import { errorDesdeProblemDetails, ErrorDeApi } from '@/api/errorDeApi'
import type { ProblemDetails } from '@/types/problemDetails'

type OpcionesDePeticion = {
  metodo?: 'GET' | 'POST' | 'PUT' | 'DELETE'
  cuerpo?: unknown
  requiereAutenticacion?: boolean
  senal?: AbortSignal
}

type ConexionConLaSesion = {
  obtenerToken: () => string | null
  alExpirarSesion: () => void
}

const urlBase = (import.meta.env.VITE_API_URL ?? 'http://localhost:5080').replace(/\/+$/, '')

let sesion: ConexionConLaSesion = {
  obtenerToken: () => null,
  alExpirarSesion: () => {},
}

export function conectarClienteConLaSesion(conexion: ConexionConLaSesion): void {
  sesion = conexion
}

async function leerProblemDetails(respuesta: Response): Promise<ProblemDetails | null> {
  const tipo = respuesta.headers.get('content-type') ?? ''

  if (!tipo.includes('json')) {
    return null
  }

  try {
    return (await respuesta.json()) as ProblemDetails
  } catch {
    return null
  }
}

export async function peticion<T>(ruta: string, opciones: OpcionesDePeticion = {}): Promise<T> {
  const { metodo = 'GET', cuerpo, requiereAutenticacion = true, senal } = opciones

  const cabeceras = new Headers({ Accept: 'application/json' })

  if (cuerpo !== undefined) {
    cabeceras.set('Content-Type', 'application/json')
  }

  if (requiereAutenticacion) {
    const token = sesion.obtenerToken()

    if (token) {
      cabeceras.set('Authorization', `Bearer ${token}`)
    }
  }

  let respuesta: Response

  try {
    respuesta = await fetch(`${urlBase}${ruta}`, {
      method: metodo,
      headers: cabeceras,
      body: cuerpo === undefined ? undefined : JSON.stringify(cuerpo),
      signal: senal,
    })
  } catch (causa) {
    if (causa instanceof DOMException && causa.name === 'AbortError') {
      throw causa
    }

    throw errorDesdeProblemDetails(0, null)
  }

  if (respuesta.status === 401 && requiereAutenticacion) {
    sesion.alExpirarSesion()
  }

  if (!respuesta.ok) {
    throw errorDesdeProblemDetails(respuesta.status, await leerProblemDetails(respuesta))
  }

  if (respuesta.status === 204) {
    return undefined as T
  }

  try {
    return (await respuesta.json()) as T
  } catch {
    throw new ErrorDeApi(respuesta.status, 'Respuesta inválida', 'El servidor devolvió una respuesta ilegible.')
  }
}
