import type { ProblemDetails } from '@/types/problemDetails'

export class ErrorDeApi extends Error {
  readonly estado: number
  readonly titulo: string
  readonly traceId?: string

  constructor(estado: number, titulo: string, detalle: string, traceId?: string) {
    super(detalle)
    this.name = 'ErrorDeApi'
    this.estado = estado
    this.titulo = titulo
    this.traceId = traceId
  }

  get esDeRed(): boolean {
    return this.estado === 0
  }

  get esNoAutorizado(): boolean {
    return this.estado === 401
  }

  get esConflicto(): boolean {
    return this.estado === 409
  }

  get esNoEncontrado(): boolean {
    return this.estado === 404
  }
}

const mensajesPorDefecto: Record<number, string> = {
  0: 'No se pudo contactar con el servidor. Verifica que la API esté disponible.',
  401: 'Tu sesión no es válida o expiró.',
  403: 'No tienes permisos para realizar esta acción.',
  404: 'El recurso solicitado no existe.',
  409: 'La operación entra en conflicto con datos existentes.',
  429: 'Se superó el límite de peticiones. Espera un momento e intenta de nuevo.',
  503: 'El servicio no está disponible en este momento.',
}

export function errorDesdeProblemDetails(estado: number, problema: ProblemDetails | null): ErrorDeApi {
  const detalle =
    problema?.detail ??
    problema?.title ??
    mensajesPorDefecto[estado] ??
    'Ocurrió un error inesperado al procesar la solicitud.'

  return new ErrorDeApi(estado, problema?.title ?? 'Error', detalle, problema?.traceId)
}
