import type { EstadoPedido } from '@/types/pedido'

const porEstado: Record<EstadoPedido, { estilo: string; texto: string }> = {
  Registrado: { estilo: 'bg-slate-100 text-slate-700 ring-slate-200', texto: 'Registrado' },
  EnProceso: { estilo: 'bg-amber-50 text-amber-800 ring-amber-200', texto: 'En proceso' },
  Enviado: { estilo: 'bg-marca-50 text-marca-700 ring-marca-200', texto: 'Enviado' },
  Entregado: { estilo: 'bg-emerald-50 text-emerald-800 ring-emerald-200', texto: 'Entregado' },
  Cancelado: { estilo: 'bg-red-50 text-red-700 ring-red-200', texto: 'Cancelado' },
}

export function DistintivoDeEstado({ estado }: { estado: EstadoPedido }) {
  const { estilo, texto } = porEstado[estado] ?? porEstado.Registrado

  return (
    <span
      className={[
        'inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ring-1 ring-inset',
        estilo,
      ].join(' ')}
    >
      {texto}
    </span>
  )
}
