import { Link } from 'react-router-dom'
import { Boton } from '@/components/ui/Boton'
import { DistintivoDeEstado } from '@/features/pedidos/componentes/DistintivoDeEstado'
import { formatearFecha, formatearMonto } from '@/lib/formato'
import { rutas } from '@/routes/rutas'
import type { Pedido } from '@/types/pedido'

type TarjetaDePedidoProps = {
  pedido: Pedido
  onEliminar: (pedido: Pedido) => void
}

export function TarjetaDePedido({ pedido, onEliminar }: TarjetaDePedidoProps) {
  return (
    <li className="space-y-3 px-4 py-4">
      <div className="flex items-start justify-between gap-3">
        <div>
          <p className="font-medium text-slate-900">{pedido.numeroPedido}</p>
          <p className="text-sm text-slate-600">{pedido.cliente}</p>
        </div>

        <DistintivoDeEstado estado={pedido.estado} />
      </div>

      <div className="flex items-center justify-between text-sm">
        <span className="text-slate-500">{formatearFecha(pedido.fecha)}</span>
        <span className="font-medium text-slate-900 tabular-nums">
          {formatearMonto(pedido.total)}
        </span>
      </div>

      <div className="flex justify-end gap-1 border-t border-slate-100 pt-2">
        <Link
          to={rutas.editarPedido(pedido.id)}
          className="rounded-md px-2.5 py-1.5 text-xs font-medium text-marca-700 transition-colors hover:bg-marca-50"
        >
          Editar
        </Link>

        <Boton
          variante="fantasma"
          tamano="pequeno"
          className="text-red-600 hover:bg-red-50 hover:text-red-700"
          onClick={() => onEliminar(pedido)}
        >
          Eliminar
        </Boton>
      </div>
    </li>
  )
}
