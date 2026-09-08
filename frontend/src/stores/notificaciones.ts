import { create } from 'zustand'

export type TonoDeNotificacion = 'exito' | 'error' | 'informacion'

export type Notificacion = {
  id: number
  tono: TonoDeNotificacion
  mensaje: string
}

type EstadoNotificaciones = {
  notificaciones: Notificacion[]
  mostrar: (tono: TonoDeNotificacion, mensaje: string) => number
  descartar: (id: number) => void
}

let siguienteId = 1

export const useNotificaciones = create<EstadoNotificaciones>()((set) => ({
  notificaciones: [],

  mostrar: (tono, mensaje) => {
    const id = siguienteId++

    set((estado) => ({ notificaciones: [...estado.notificaciones, { id, tono, mensaje }] }))

    return id
  },

  descartar: (id) =>
    set((estado) => ({
      notificaciones: estado.notificaciones.filter((notificacion) => notificacion.id !== id),
    })),
}))

export function notificarExito(mensaje: string): number {
  return useNotificaciones.getState().mostrar('exito', mensaje)
}

export function notificarError(mensaje: string): number {
  return useNotificaciones.getState().mostrar('error', mensaje)
}
