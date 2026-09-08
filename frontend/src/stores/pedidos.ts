import { create } from 'zustand'
import { ErrorDeApi } from '@/api/errorDeApi'
import { obtenerPedidos } from '@/api/pedidos'
import type { Pedido } from '@/types/pedido'

type EstadoPedidos = {
  pedidos: Pedido[]
  cargando: boolean
  cargados: boolean
  error: string | null
  cargarPedidos: (senal?: AbortSignal) => Promise<void>
  reemplazarPedido: (pedido: Pedido) => void
  quitarPedido: (id: number) => void
}

function esCancelacion(causa: unknown): boolean {
  return causa instanceof DOMException && causa.name === 'AbortError'
}

export const usePedidos = create<EstadoPedidos>()((set) => ({
  pedidos: [],
  cargando: false,
  cargados: false,
  error: null,

  cargarPedidos: async (senal) => {
    set({ cargando: true, error: null })

    try {
      const pedidos = await obtenerPedidos(senal)

      set({ pedidos, cargando: false, cargados: true, error: null })
    } catch (causa) {
      if (esCancelacion(causa)) {
        return
      }

      const error =
        causa instanceof ErrorDeApi ? causa.message : 'No se pudieron cargar los pedidos.'

      set({ cargando: false, error })
    }
  },

  reemplazarPedido: (pedido) =>
    set((estado) => ({
      pedidos: estado.pedidos.map((actual) => (actual.id === pedido.id ? pedido : actual)),
    })),

  quitarPedido: (id) =>
    set((estado) => ({ pedidos: estado.pedidos.filter((pedido) => pedido.id !== id) })),
}))
