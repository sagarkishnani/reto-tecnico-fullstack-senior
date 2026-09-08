import { etiquetaDeEstado } from '@/features/pedidos/etiquetasDeEstado'
import type { EstadoPedido } from '@/types/pedido'

const estilos: Record<EstadoPedido, string> = {
  Registrado: 'bg-slate-100 text-slate-700 ring-slate-200',
  EnProceso: 'bg-amber-50 text-amber-800 ring-amber-200',
  Enviado: 'bg-marca-50 text-marca-700 ring-marca-200',
  Entregado: 'bg-emerald-50 text-emerald-800 ring-emerald-200',
  Cancelado: 'bg-red-50 text-red-700 ring-red-200',
}

export function DistintivoDeEstado({ estado }: { estado: EstadoPedido }) {
  return (
    <span
      className={[
        'inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ring-1 ring-inset',
        estilos[estado] ?? estilos.Registrado,
      ].join(' ')}
    >
      {etiquetaDeEstado(estado)}
    </span>
  )
}
