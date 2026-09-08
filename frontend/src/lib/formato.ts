const formateadorDeMonto = new Intl.NumberFormat('es-PE', {
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
})

const formateadorDeFecha = new Intl.DateTimeFormat('es-PE', {
  day: '2-digit',
  month: 'short',
  year: 'numeric',
  timeZone: 'UTC',
})

export function formatearMonto(valor: number): string {
  return formateadorDeMonto.format(valor)
}

export function formatearFecha(fechaIso: string): string {
  const fecha = new Date(fechaIso)

  if (Number.isNaN(fecha.getTime())) {
    return '—'
  }

  return formateadorDeFecha.format(fecha)
}

export function aFechaDeFormulario(fechaIso: string): string {
  const fecha = new Date(fechaIso)

  if (Number.isNaN(fecha.getTime())) {
    return ''
  }

  return fecha.toISOString().slice(0, 10)
}
