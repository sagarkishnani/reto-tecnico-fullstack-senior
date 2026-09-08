import { estadosDePedido, type EstadoPedido } from '@/types/pedido'

export const limites = {
  longitudNumeroPedido: 50,
  longitudCliente: 150,
  totalMaximo: 99_999_999.99,
} as const

export type BorradorDePedido = {
  numeroPedido: string
  cliente: string
  fecha: string
  total: string
  estado: EstadoPedido
}

export type ErroresDePedido = Partial<Record<keyof BorradorDePedido, string>>

export function normalizarNumeroPedido(valor: string): string {
  return valor.trim().toUpperCase()
}

export function interpretarTotal(valor: string): number | null {
  const limpio = valor.trim().replace(',', '.')

  if (limpio === '' || !/^\d*\.?\d*$/.test(limpio)) {
    return null
  }

  const numero = Number(limpio)

  return Number.isFinite(numero) ? numero : null
}

export function validarBorrador(borrador: BorradorDePedido, esEdicion: boolean): ErroresDePedido {
  const errores: ErroresDePedido = {}

  const numeroPedido = normalizarNumeroPedido(borrador.numeroPedido)

  if (numeroPedido === '') {
    errores.numeroPedido = 'El número de pedido es obligatorio.'
  } else if (numeroPedido.length > limites.longitudNumeroPedido) {
    errores.numeroPedido = `No puede superar los ${limites.longitudNumeroPedido} caracteres.`
  }

  const cliente = borrador.cliente.trim()

  if (cliente === '') {
    errores.cliente = 'El cliente es obligatorio.'
  } else if (cliente.length > limites.longitudCliente) {
    errores.cliente = `No puede superar los ${limites.longitudCliente} caracteres.`
  }

  if (borrador.fecha.trim() === '') {
    errores.fecha = 'La fecha es obligatoria.'
  } else if (Number.isNaN(new Date(borrador.fecha).getTime())) {
    errores.fecha = 'La fecha no es válida.'
  }

  const total = interpretarTotal(borrador.total)

  if (total === null) {
    errores.total = 'Escribe un monto numérico.'
  } else if (total <= 0) {
    errores.total = 'El total debe ser mayor que cero.'
  } else if (total > limites.totalMaximo) {
    errores.total = `El total no puede superar ${limites.totalMaximo.toLocaleString('es-PE')}.`
  }

  if (esEdicion && !estadosDePedido.includes(borrador.estado)) {
    errores.estado = 'Selecciona un estado válido.'
  }

  return errores
}

export function hayErrores(errores: ErroresDePedido): boolean {
  return Object.keys(errores).length > 0
}
