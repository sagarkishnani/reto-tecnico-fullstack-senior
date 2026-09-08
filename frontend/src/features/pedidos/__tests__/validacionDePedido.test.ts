import { describe, expect, it } from 'vitest'
import {
  interpretarTotal,
  limites,
  normalizarNumeroPedido,
  validarBorrador,
  type BorradorDePedido,
} from '@/features/pedidos/validacionDePedido'

function borrador(parciales: Partial<BorradorDePedido> = {}): BorradorDePedido {
  return {
    numeroPedido: 'PED-100',
    cliente: 'Juan Perez',
    fecha: '2025-01-10',
    total: '250.75',
    estado: 'Registrado',
    ...parciales,
  }
}

describe('normalizarNumeroPedido', () => {
  it.each([
    ['ped-001', 'PED-001'],
    ['  PED-002  ', 'PED-002'],
    ['Ped-003', 'PED-003'],
  ])('normaliza %s a %s igual que el dominio del backend', (entrada, esperado) => {
    expect(normalizarNumeroPedido(entrada)).toBe(esperado)
  })
})

describe('interpretarTotal', () => {
  it.each([
    ['250.75', 250.75],
    ['250,75', 250.75],
    ['  10  ', 10],
    ['0', 0],
  ])('interpreta %s como %s', (entrada, esperado) => {
    expect(interpretarTotal(entrada)).toBe(esperado)
  })

  it.each([['', 'vacio'], ['abc', 'texto'], ['1.2.3', 'dos separadores'], ['-5', 'signo negativo']])(
    'devuelve null ante %s',
    (entrada) => {
      expect(interpretarTotal(entrada)).toBeNull()
    },
  )
})

describe('validarBorrador', () => {
  it('acepta un borrador correcto', () => {
    expect(validarBorrador(borrador(), false)).toEqual({})
  })

  it.each([
    ['0', 'mayor que cero'],
    ['-10', 'numérico'],
    ['', 'numérico'],
    ['abc', 'numérico'],
  ])('rechaza el total %s', (total, fragmento) => {
    expect(validarBorrador(borrador({ total }), false).total).toContain(fragmento)
  })

  it('rechaza un total por encima del maximo de la columna decimal', () => {
    const excedido = String(limites.totalMaximo + 1)

    expect(validarBorrador(borrador({ total: excedido }), false).total).toContain('no puede superar')
  })

  it('acepta exactamente el total maximo', () => {
    expect(validarBorrador(borrador({ total: String(limites.totalMaximo) }), false).total).toBeUndefined()
  })

  it.each([
    ['numeroPedido', '   '],
    ['cliente', ''],
    ['fecha', ''],
  ])('exige %s', (campo, valor) => {
    const errores = validarBorrador(borrador({ [campo]: valor }), false)

    expect(errores[campo as keyof typeof errores]).toContain('obligatori')
  })

  it('rechaza un numero de pedido mas largo que el limite del backend', () => {
    const largo = 'X'.repeat(limites.longitudNumeroPedido + 1)

    expect(validarBorrador(borrador({ numeroPedido: largo }), false).numeroPedido).toBeDefined()
  })

  it('rechaza un cliente mas largo que el limite del backend', () => {
    const largo = 'X'.repeat(limites.longitudCliente + 1)

    expect(validarBorrador(borrador({ cliente: largo }), false).cliente).toBeDefined()
  })

  it('rechaza una fecha invalida', () => {
    expect(validarBorrador(borrador({ fecha: '2025-13-45' }), false).fecha).toBeDefined()
  })

  it('no valida el estado al crear, porque el backend siempre asigna Registrado', () => {
    const errores = validarBorrador(borrador({ estado: 'Inventado' as never }), false)

    expect(errores.estado).toBeUndefined()
  })

  it('valida el estado al editar', () => {
    const errores = validarBorrador(borrador({ estado: 'Inventado' as never }), true)

    expect(errores.estado).toBeDefined()
  })
})
