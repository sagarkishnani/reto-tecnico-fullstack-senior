export const estadosDePedido = [
  'Registrado',
  'EnProceso',
  'Enviado',
  'Entregado',
  'Cancelado',
] as const

export type EstadoPedido = (typeof estadosDePedido)[number]

export type Pedido = {
  id: number
  numeroPedido: string
  cliente: string
  fecha: string
  total: number
  estado: EstadoPedido
}

export type CrearPedidoRequest = {
  numeroPedido: string
  cliente: string
  fecha: string
  total: number
}

export type ActualizarPedidoRequest = CrearPedidoRequest & {
  estado: EstadoPedido
}
