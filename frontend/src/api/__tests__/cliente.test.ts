import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { conectarClienteConLaSesion, peticion } from '@/api/cliente'
import { ErrorDeApi } from '@/api/errorDeApi'

function respuestaJson(cuerpo: unknown, estado = 200): Response {
  return new Response(JSON.stringify(cuerpo), {
    status: estado,
    headers: { 'content-type': 'application/json' },
  })
}

const alExpirarSesion = vi.fn()
let tokenActual: string | null = null

beforeEach(() => {
  tokenActual = 'token-de-prueba'
  alExpirarSesion.mockClear()
  conectarClienteConLaSesion({
    obtenerToken: () => tokenActual,
    alExpirarSesion,
  })
})

afterEach(() => {
  vi.unstubAllGlobals()
})

async function capturar(promesa: Promise<unknown>): Promise<unknown> {
  try {
    await promesa
  } catch (causa) {
    return causa
  }

  throw new Error('Se esperaba que la petición fallara.')
}

async function capturarErrorDeApi(promesa: Promise<unknown>): Promise<ErrorDeApi> {
  const causa = await capturar(promesa)

  if (!(causa instanceof ErrorDeApi)) {
    throw new Error(`Se esperaba un ErrorDeApi y llegó ${String(causa)}.`)
  }

  return causa
}

function simularFetch(implementacion: (url: string, init: RequestInit) => Promise<Response>) {
  const espia = vi.fn(implementacion)
  vi.stubGlobal('fetch', espia)

  return espia
}

describe('peticion', () => {
  it('adjunta el token como cabecera Authorization', async () => {
    const espia = simularFetch(async () => respuestaJson([{ id: 1 }]))

    await peticion('/api/pedidos')

    const cabeceras = new Headers(espia.mock.calls[0][1].headers)
    expect(cabeceras.get('Authorization')).toBe('Bearer token-de-prueba')
  })

  it('no adjunta token cuando la peticion es publica', async () => {
    const espia = simularFetch(async () => respuestaJson({ token: 'x', expiresIn: 3600 }))

    await peticion('/auth/login', { metodo: 'POST', cuerpo: {}, requiereAutenticacion: false })

    const cabeceras = new Headers(espia.mock.calls[0][1].headers)
    expect(cabeceras.has('Authorization')).toBe(false)
  })

  it('serializa el cuerpo y declara el tipo de contenido', async () => {
    const espia = simularFetch(async () => respuestaJson({ id: 1 }))

    await peticion('/api/pedidos', { metodo: 'POST', cuerpo: { total: 10 } })

    const init = espia.mock.calls[0][1]
    expect(init.body).toBe('{"total":10}')
    expect(new Headers(init.headers).get('Content-Type')).toBe('application/json')
  })

  it('traduce ProblemDetails a un ErrorDeApi con el detalle del servidor', async () => {
    simularFetch(async () =>
      respuestaJson(
        {
          title: 'Conflicto de unicidad',
          status: 409,
          detail: 'Ya existe un pedido con el número PED-001.',
          traceId: '00-abc-123',
        },
        409,
      ),
    )

    const error = await capturarErrorDeApi(peticion('/api/pedidos', { metodo: 'POST', cuerpo: {} }))

    expect(error.estado).toBe(409)
    expect(error.esConflicto).toBe(true)
    expect(error.message).toBe('Ya existe un pedido con el número PED-001.')
    expect(error.traceId).toBe('00-abc-123')
  })

  it('usa un mensaje por defecto cuando la respuesta de error no trae cuerpo', async () => {
    simularFetch(async () => new Response(null, { status: 503 }))

    const error = await capturarErrorDeApi(peticion('/api/pedidos'))

    expect(error.estado).toBe(503)
    expect(error.message).toContain('no está disponible')
  })

  it('cierra la sesion cuando el servidor responde 401', async () => {
    simularFetch(async () => new Response(null, { status: 401 }))

    await peticion('/api/pedidos').catch(() => {})

    expect(alExpirarSesion).toHaveBeenCalledTimes(1)
  })

  it('no cierra la sesion cuando el 401 viene de un login fallido', async () => {
    simularFetch(async () => new Response(null, { status: 401 }))

    await peticion('/auth/login', { requiereAutenticacion: false }).catch(() => {})

    expect(alExpirarSesion).not.toHaveBeenCalled()
  })

  it('convierte un fallo de red en un ErrorDeApi con estado 0', async () => {
    simularFetch(async () => {
      throw new TypeError('Failed to fetch')
    })

    const error = await capturarErrorDeApi(peticion('/api/pedidos'))

    expect(error.esDeRed).toBe(true)
    expect(error.message).toContain('No se pudo contactar')
  })

  it('devuelve undefined ante un 204 sin intentar leer json', async () => {
    simularFetch(async () => new Response(null, { status: 204 }))

    await expect(peticion('/api/pedidos/1', { metodo: 'DELETE' })).resolves.toBeUndefined()
  })

  it('propaga la cancelacion sin convertirla en error de red', async () => {
    simularFetch(async () => {
      throw new DOMException('Abortada', 'AbortError')
    })

    const error = await capturar(peticion('/api/pedidos'))

    expect(error).toBeInstanceOf(DOMException)
    expect((error as DOMException).name).toBe('AbortError')
  })
})
