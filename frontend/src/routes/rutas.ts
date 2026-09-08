export const rutas = {
  login: '/login',
  pedidos: '/pedidos',
  nuevoPedido: '/pedidos/nuevo',
  editarPedido: (id: number | string) => `/pedidos/${id}/editar`,
} as const

export const patrones = {
  editarPedido: '/pedidos/:id/editar',
} as const
