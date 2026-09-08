import { beforeEach, describe, expect, it, vi } from 'vitest'
import { ErrorDeApi } from '@/api/errorDeApi'
import { usePedidos } from '@/stores/pedidos'
import type { Pedido } from '@/types/pedido'

const obtenerPedidosDeApi = vi.hoisted(() => vi.fn())

vi.mock('@/api/pedidos', () => ({ obtenerPedidos: obtenerPedidosDeApi }))

function pedido(parciales: Partial<Pedido> = {}): Pedido {
  return {
    id: 1,
    numeroPedido: 'PED-001',
    cliente: 'Juan Perez',
    fecha: '2025-01-10T00:00:00Z',
    total: 250.75,
    estado: 'Registrado',
    ...parciales,
  }
}

beforeEach(() => {
  obtenerPedidosDeApi.mockReset()
  usePedidos.setState({ pedidos: [], cargando: false, cargados: false, error: null })
})

describe('cargarPedidos', () => {
  it('guarda los pedidos y marca la carga como completada', async () => {
    obtenerPedidosDeApi.mockResolvedValue([pedido(), pedido({ id: 2, numeroPedido: 'PED-002' })])

    await usePedidos.getState().cargarPedidos()

    expect(usePedidos.getState().pedidos).toHaveLength(2)
    expect(usePedidos.getState().cargados).toBe(true)
    expect(usePedidos.getState().cargando).toBe(false)
    expect(usePedidos.getState().error).toBeNull()
  })

  it('expone el mensaje del servidor cuando la peticion falla', async () => {
    obtenerPedidosDeApi.mockRejectedValue(
      new ErrorDeApi(503, 'Servicio no disponible', 'La base de datos no está respondiendo.'),
    )

    await usePedidos.getState().cargarPedidos()

    expect(usePedidos.getState().error).toBe('La base de datos no está respondiendo.')
    expect(usePedidos.getState().cargando).toBe(false)
    expect(usePedidos.getState().cargados).toBe(false)
  })

  it('ignora una cancelacion sin mostrar error ni apagar el indicador de carga', async () => {
    obtenerPedidosDeApi.mockRejectedValue(new DOMException('Abortada', 'AbortError'))

    await usePedidos.getState().cargarPedidos()

    expect(usePedidos.getState().error).toBeNull()
    expect(usePedidos.getState().cargando).toBe(true)
  })

  it('limpia un error previo al reintentar', async () => {
    usePedidos.setState({ error: 'error anterior' })
    obtenerPedidosDeApi.mockResolvedValue([pedido()])

    await usePedidos.getState().cargarPedidos()

    expect(usePedidos.getState().error).toBeNull()
  })

  it('propaga la senal de cancelacion a la capa de api', async () => {
    obtenerPedidosDeApi.mockResolvedValue([])
    const controlador = new AbortController()

    await usePedidos.getState().cargarPedidos(controlador.signal)

    expect(obtenerPedidosDeApi).toHaveBeenCalledWith(controlador.signal)
  })
})

describe('reemplazarPedido', () => {
  it('sustituye solo el pedido con el mismo id', () => {
    usePedidos.setState({ pedidos: [pedido(), pedido({ id: 2, numeroPedido: 'PED-002' })] })

    usePedidos.getState().reemplazarPedido(pedido({ id: 2, numeroPedido: 'PED-002', cliente: 'Ana Diaz' }))

    expect(usePedidos.getState().pedidos[0].cliente).toBe('Juan Perez')
    expect(usePedidos.getState().pedidos[1].cliente).toBe('Ana Diaz')
  })
})

describe('quitarPedido', () => {
  it('elimina el pedido de la lista sin tocar los demas', () => {
    usePedidos.setState({ pedidos: [pedido(), pedido({ id: 2 })] })

    usePedidos.getState().quitarPedido(1)

    expect(usePedidos.getState().pedidos).toHaveLength(1)
    expect(usePedidos.getState().pedidos[0].id).toBe(2)
  })
})
