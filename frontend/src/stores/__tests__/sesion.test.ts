import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { ErrorDeApi } from '@/api/errorDeApi'
import { claveDeAlmacenamiento, haySesionActiva, useSesion } from '@/stores/sesion'

const iniciarSesionEnApi = vi.hoisted(() => vi.fn())

vi.mock('@/api/autenticacion', () => ({ iniciarSesion: iniciarSesionEnApi }))

function construirToken(carga: Record<string, unknown>): string {
  const codificar = (valor: unknown) => {
    const bytes = new TextEncoder().encode(JSON.stringify(valor))
    const binario = Array.from(bytes, (byte) => String.fromCharCode(byte)).join('')

    return btoa(binario).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '')
  }

  return `${codificar({ alg: 'HS256' })}.${codificar(carga)}.firma`
}

const tokenValido = construirToken({
  sub: '1',
  email: 'user@email.com',
  name: 'Usuario Demo',
  role: 'Admin',
  exp: Math.floor(Date.now() / 1000) + 3600,
})

const tokenExpirado = construirToken({
  sub: '1',
  email: 'user@email.com',
  role: 'User',
  exp: Math.floor(Date.now() / 1000) - 60,
})

const credenciales = { email: 'user@email.com', password: '123456' }

beforeEach(() => {
  localStorage.clear()
  iniciarSesionEnApi.mockReset()
  useSesion.setState({ token: null, usuario: null, autenticando: false, error: null })
})

afterEach(() => {
  localStorage.clear()
})

describe('iniciarSesion', () => {
  it('guarda el token y los datos del usuario extraidos del propio token', async () => {
    iniciarSesionEnApi.mockResolvedValue({ token: tokenValido, expiresIn: 3600 })

    const correcto = await useSesion.getState().iniciarSesion(credenciales)

    expect(correcto).toBe(true)
    expect(useSesion.getState().token).toBe(tokenValido)
    expect(useSesion.getState().usuario?.email).toBe('user@email.com')
    expect(useSesion.getState().usuario?.rol).toBe('Admin')
    expect(useSesion.getState().error).toBeNull()
  })

  it('persiste unicamente el token, no los datos derivados', async () => {
    iniciarSesionEnApi.mockResolvedValue({ token: tokenValido, expiresIn: 3600 })

    await useSesion.getState().iniciarSesion(credenciales)

    const guardado = JSON.parse(localStorage.getItem(claveDeAlmacenamiento) ?? '{}')
    expect(guardado.state).toEqual({ token: tokenValido })
  })

  it('expone el mensaje del servidor cuando las credenciales son invalidas', async () => {
    iniciarSesionEnApi.mockRejectedValue(
      new ErrorDeApi(401, 'Credenciales inválidas', 'Las credenciales proporcionadas no son válidas.'),
    )

    const correcto = await useSesion.getState().iniciarSesion(credenciales)

    expect(correcto).toBe(false)
    expect(useSesion.getState().token).toBeNull()
    expect(useSesion.getState().error).toBe('Las credenciales proporcionadas no son válidas.')
  })

  it('rechaza un token expirado aunque el servidor responda 200', async () => {
    iniciarSesionEnApi.mockResolvedValue({ token: tokenExpirado, expiresIn: 3600 })

    const correcto = await useSesion.getState().iniciarSesion(credenciales)

    expect(correcto).toBe(false)
    expect(useSesion.getState().token).toBeNull()
    expect(useSesion.getState().error).toContain('token inválido')
  })

  it('marca autenticando mientras la peticion esta en curso', async () => {
    let resolver: (valor: unknown) => void = () => {}
    iniciarSesionEnApi.mockReturnValue(new Promise((resuelve) => (resolver = resuelve)))

    const promesa = useSesion.getState().iniciarSesion(credenciales)
    expect(useSesion.getState().autenticando).toBe(true)

    resolver({ token: tokenValido, expiresIn: 3600 })
    await promesa

    expect(useSesion.getState().autenticando).toBe(false)
  })

  it('limpia un error previo al reintentar', async () => {
    useSesion.setState({ error: 'error anterior' })
    iniciarSesionEnApi.mockResolvedValue({ token: tokenValido, expiresIn: 3600 })

    await useSesion.getState().iniciarSesion(credenciales)

    expect(useSesion.getState().error).toBeNull()
  })
})

describe('cerrarSesion', () => {
  it('borra el token y el usuario', async () => {
    iniciarSesionEnApi.mockResolvedValue({ token: tokenValido, expiresIn: 3600 })
    await useSesion.getState().iniciarSesion(credenciales)

    useSesion.getState().cerrarSesion()

    expect(useSesion.getState().token).toBeNull()
    expect(useSesion.getState().usuario).toBeNull()
  })

  it('elimina el token del almacenamiento persistente', async () => {
    iniciarSesionEnApi.mockResolvedValue({ token: tokenValido, expiresIn: 3600 })
    await useSesion.getState().iniciarSesion(credenciales)

    useSesion.getState().cerrarSesion()

    const guardado = JSON.parse(localStorage.getItem(claveDeAlmacenamiento) ?? '{}')
    expect(guardado.state?.token).toBeNull()
  })
})

describe('haySesionActiva', () => {
  it('es falso sin token', () => {
    expect(haySesionActiva()).toBe(false)
  })

  it('es verdadero con un token vigente', async () => {
    iniciarSesionEnApi.mockResolvedValue({ token: tokenValido, expiresIn: 3600 })
    await useSesion.getState().iniciarSesion(credenciales)

    expect(haySesionActiva()).toBe(true)
  })
})
