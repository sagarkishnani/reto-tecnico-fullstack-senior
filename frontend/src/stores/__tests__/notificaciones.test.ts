import { beforeEach, describe, expect, it } from 'vitest'
import { notificarError, notificarExito, useNotificaciones } from '@/stores/notificaciones'

beforeEach(() => {
  useNotificaciones.setState({ notificaciones: [] })
})

describe('notificaciones', () => {
  it('apila varias notificaciones en orden de llegada', () => {
    notificarExito('primera')
    notificarError('segunda')

    expect(useNotificaciones.getState().notificaciones.map((n) => n.mensaje)).toEqual([
      'primera',
      'segunda',
    ])
  })

  it('asigna identificadores distintos', () => {
    const primera = notificarExito('a')
    const segunda = notificarExito('b')

    expect(primera).not.toBe(segunda)
  })

  it('marca el tono segun el atajo usado', () => {
    notificarExito('todo bien')
    notificarError('algo falló')

    const [exito, error] = useNotificaciones.getState().notificaciones
    expect(exito.tono).toBe('exito')
    expect(error.tono).toBe('error')
  })

  it('descarta solo la notificacion indicada', () => {
    const primera = notificarExito('primera')
    notificarExito('segunda')

    useNotificaciones.getState().descartar(primera)

    expect(useNotificaciones.getState().notificaciones.map((n) => n.mensaje)).toEqual(['segunda'])
  })

  it('descartar un identificador inexistente no altera la lista', () => {
    notificarExito('unica')

    useNotificaciones.getState().descartar(9999)

    expect(useNotificaciones.getState().notificaciones).toHaveLength(1)
  })
})
