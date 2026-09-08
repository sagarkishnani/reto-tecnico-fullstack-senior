import { peticion } from '@/api/cliente'
import type { ActualizarPedidoRequest, CrearPedidoRequest, Pedido } from '@/types/pedido'

const recurso = '/api/pedidos'

export function obtenerPedidos(senal?: AbortSignal): Promise<Pedido[]> {
  return peticion<Pedido[]>(recurso, { senal })
}

export function obtenerPedidoPorId(id: number, senal?: AbortSignal): Promise<Pedido> {
  return peticion<Pedido>(`${recurso}/${id}`, { senal })
}

export function crearPedido(datos: CrearPedidoRequest): Promise<Pedido> {
  return peticion<Pedido>(recurso, { metodo: 'POST', cuerpo: datos })
}

export function actualizarPedido(id: number, datos: ActualizarPedidoRequest): Promise<Pedido> {
  return peticion<Pedido>(`${recurso}/${id}`, { metodo: 'PUT', cuerpo: datos })
}

export function eliminarPedido(id: number): Promise<void> {
  return peticion<void>(`${recurso}/${id}`, { metodo: 'DELETE' })
}
