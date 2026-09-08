import type { EstadoPedido } from '@/types/pedido'

const etiquetas: Record<EstadoPedido, string> = {
  Registrado: 'Registrado',
  EnProceso: 'En proceso',
  Enviado: 'Enviado',
  Entregado: 'Entregado',
  Cancelado: 'Cancelado',
}

export function etiquetaDeEstado(estado: EstadoPedido): string {
  return etiquetas[estado] ?? estado
}
