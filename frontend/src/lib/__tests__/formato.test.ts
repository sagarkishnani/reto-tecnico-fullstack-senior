import { describe, expect, it } from 'vitest'
import { aFechaDeFormulario, formatearFecha, formatearMonto } from '@/lib/formato'

describe('formatearMonto', () => {
  it.each([
    [250.75, '250.75'],
    [1299, '1,299.00'],
    [4310.2, '4,310.20'],
    [0.5, '0.50'],
  ])('formatea %s con dos decimales y separador de miles', (valor, esperado) => {
    expect(formatearMonto(valor)).toBe(esperado)
  })
})

describe('formatearFecha', () => {
  it('no desplaza el dia al formatear una fecha en UTC', () => {
    expect(formatearFecha('2025-01-10T00:00:00Z')).toContain('10')
  })

  it('conserva el dia aunque la zona local este por detras de UTC', () => {
    expect(formatearFecha('2026-09-02T00:00:00Z')).toContain('02')
  })

  it('devuelve un guion ante una fecha invalida', () => {
    expect(formatearFecha('no-es-una-fecha')).toBe('—')
  })
})

describe('aFechaDeFormulario', () => {
  it('recorta la marca de tiempo al formato que espera un input date', () => {
    expect(aFechaDeFormulario('2025-01-10T00:00:00Z')).toBe('2025-01-10')
  })

  it('devuelve cadena vacia ante una fecha invalida', () => {
    expect(aFechaDeFormulario('')).toBe('')
  })
})
