import { useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { ErrorDeApi } from '@/api/errorDeApi'
import { Alerta } from '@/components/ui/Alerta'
import { Boton } from '@/components/ui/Boton'
import { DialogoDeConfirmacion } from '@/components/ui/DialogoDeConfirmacion'
import { EstadoVacio } from '@/components/ui/EstadoVacio'
import { DistintivoDeEstado } from '@/features/pedidos/componentes/DistintivoDeEstado'
import { FilasDeCarga } from '@/features/pedidos/componentes/FilasDeCarga'
import { TarjetaDePedido } from '@/features/pedidos/componentes/TarjetaDePedido'
import { etiquetaDeEstado } from '@/features/pedidos/etiquetasDeEstado'
import { formatearFecha, formatearMonto } from '@/lib/formato'
import { rutas } from '@/routes/rutas'
import { notificarError, notificarExito } from '@/stores/notificaciones'
import { usePedidos } from '@/stores/pedidos'
import { estadosDePedido, type EstadoPedido, type Pedido } from '@/types/pedido'

const columnas = ['Número', 'Cliente', 'Fecha', 'Total', 'Estado', ''] as const

type FiltroDeEstado = EstadoPedido | 'todos'

export function PaginaListaPedidos() {
  const pedidos = usePedidos((estado) => estado.pedidos)
  const cargando = usePedidos((estado) => estado.cargando)
  const cargados = usePedidos((estado) => estado.cargados)
  const error = usePedidos((estado) => estado.error)
  const cargarPedidos = usePedidos((estado) => estado.cargarPedidos)
  const eliminar = usePedidos((estado) => estado.eliminar)

  const [busqueda, setBusqueda] = useState('')
  const [filtroDeEstado, setFiltroDeEstado] = useState<FiltroDeEstado>('todos')
  const [pedidoAEliminar, setPedidoAEliminar] = useState<Pedido | null>(null)
  const [eliminando, setEliminando] = useState(false)

  useEffect(() => {
    const controlador = new AbortController()

    void cargarPedidos(controlador.signal)

    return () => controlador.abort()
  }, [cargarPedidos])

  const visibles = useMemo(() => {
    const termino = busqueda.trim().toLowerCase()

    return pedidos.filter((pedido) => {
      const coincideTexto =
        termino === '' ||
        pedido.numeroPedido.toLowerCase().includes(termino) ||
        pedido.cliente.toLowerCase().includes(termino)

      const coincideEstado = filtroDeEstado === 'todos' || pedido.estado === filtroDeEstado

      return coincideTexto && coincideEstado
    })
  }, [pedidos, busqueda, filtroDeEstado])

  const hayFiltrosActivos = busqueda.trim() !== '' || filtroDeEstado !== 'todos'
  const mostrarContenido = cargados || (cargando && !error)

  const confirmarEliminacion = async () => {
    if (!pedidoAEliminar) {
      return
    }

    setEliminando(true)

    try {
      await eliminar(pedidoAEliminar.id)
      notificarExito(`Pedido ${pedidoAEliminar.numeroPedido} eliminado.`)
      setPedidoAEliminar(null)
    } catch (causa) {
      notificarError(
        causa instanceof ErrorDeApi ? causa.message : 'No se pudo eliminar el pedido.',
      )
    } finally {
      setEliminando(false)
    }
  }

  return (
    <section className="space-y-5">
      <header className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <h1 className="text-xl font-semibold tracking-tight text-slate-900">Pedidos</h1>
          {!(error && !cargados) && (
            <p className="mt-0.5 text-sm text-slate-600">
              {cargados
                ? `${pedidos.length} ${pedidos.length === 1 ? 'pedido registrado' : 'pedidos registrados'}`
                : 'Cargando pedidos'}
            </p>
          )}
        </div>

        <Link
          to={rutas.nuevoPedido}
          className="inline-flex items-center gap-2 rounded-md bg-marca-600 px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-marca-700"
        >
          Nuevo pedido
        </Link>
      </header>

      {error && (
        <Alerta tono="error" titulo="No se pudieron cargar los pedidos">
          <p>{error}</p>
          <Boton
            variante="secundario"
            tamano="pequeno"
            className="mt-2"
            onClick={() => void cargarPedidos()}
          >
            Reintentar
          </Boton>
        </Alerta>
      )}

      {cargados && pedidos.length > 0 && (
        <div className="flex flex-wrap gap-3">
          <input
            type="search"
            value={busqueda}
            onChange={(evento) => setBusqueda(evento.target.value)}
            placeholder="Buscar por número o cliente"
            aria-label="Buscar pedidos"
            className="w-full max-w-xs rounded-md border border-slate-300 bg-white px-3 py-2 text-sm shadow-xs placeholder:text-slate-400 focus:border-marca-500 focus:ring-2 focus:ring-marca-100 focus:outline-none"
          />

          <select
            value={filtroDeEstado}
            onChange={(evento) => setFiltroDeEstado(evento.target.value as FiltroDeEstado)}
            aria-label="Filtrar por estado"
            className="rounded-md border border-slate-300 bg-white px-3 py-2 text-sm shadow-xs focus:border-marca-500 focus:ring-2 focus:ring-marca-100 focus:outline-none"
          >
            <option value="todos">Todos los estados</option>
            {estadosDePedido.map((estado) => (
              <option key={estado} value={estado}>
                {etiquetaDeEstado(estado)}
              </option>
            ))}
          </select>
        </div>
      )}

      {mostrarContenido && (
        <div className="overflow-hidden rounded-lg border border-slate-200 bg-white shadow-xs">
          <div className="hidden overflow-x-auto sm:block">
            <table className="w-full min-w-2xl text-sm">
              <thead className="border-b border-slate-200 bg-slate-50 text-left">
                <tr>
                  {columnas.map((columna, indice) => (
                    <th
                      key={columna || indice}
                      scope="col"
                      className={[
                        'px-4 py-2.5 text-xs font-semibold tracking-wide text-slate-600 uppercase',
                        columna === 'Total' ? 'text-right' : '',
                      ].join(' ')}
                    >
                      {columna || <span className="sr-only">Acciones</span>}
                    </th>
                  ))}
                </tr>
              </thead>

              <tbody className="divide-y divide-slate-100">
                {cargando && !cargados && <FilasDeCarga columnas={columnas.length} />}

                {cargados &&
                  visibles.map((pedido) => (
                    <tr key={pedido.id} className="transition-colors hover:bg-slate-50">
                      <td className="px-4 py-3 font-medium text-slate-900">{pedido.numeroPedido}</td>
                      <td className="px-4 py-3 text-slate-700">{pedido.cliente}</td>
                      <td className="px-4 py-3 whitespace-nowrap text-slate-600">
                        {formatearFecha(pedido.fecha)}
                      </td>
                      <td className="px-4 py-3 text-right font-medium whitespace-nowrap text-slate-900 tabular-nums">
                        {formatearMonto(pedido.total)}
                      </td>
                      <td className="px-4 py-3">
                        <DistintivoDeEstado estado={pedido.estado} />
                      </td>
                      <td className="px-4 py-3 text-right whitespace-nowrap">
                        <Link
                          to={rutas.editarPedido(pedido.id)}
                          className="rounded-md px-2.5 py-1.5 text-xs font-medium text-marca-700 transition-colors hover:bg-marca-50"
                        >
                          Editar
                        </Link>

                        <Boton
                          variante="fantasma"
                          tamano="pequeno"
                          className="ml-1 text-red-600 hover:bg-red-50 hover:text-red-700"
                          onClick={() => setPedidoAEliminar(pedido)}
                        >
                          Eliminar
                        </Boton>
                      </td>
                    </tr>
                  ))}
              </tbody>
            </table>
          </div>

          {cargados && visibles.length > 0 && (
            <ul className="divide-y divide-slate-100 sm:hidden">
              {visibles.map((pedido) => (
                <TarjetaDePedido key={pedido.id} pedido={pedido} onEliminar={setPedidoAEliminar} />
              ))}
            </ul>
          )}

          {cargados && pedidos.length === 0 && !error && (
            <EstadoVacio
              titulo="Todavía no hay pedidos"
              descripcion="Cuando registres el primer pedido aparecerá en esta lista."
              accion={
                <Link
                  to={rutas.nuevoPedido}
                  className="inline-flex items-center rounded-md bg-marca-600 px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-marca-700"
                >
                  Crear el primer pedido
                </Link>
              }
            />
          )}

          {cargados && pedidos.length > 0 && visibles.length === 0 && (
            <EstadoVacio
              titulo="Ningún pedido coincide"
              descripcion="Prueba con otro número de pedido, otro cliente u otro estado."
              accion={
                hayFiltrosActivos && (
                  <Boton
                    variante="secundario"
                    onClick={() => {
                      setBusqueda('')
                      setFiltroDeEstado('todos')
                    }}
                  >
                    Limpiar filtros
                  </Boton>
                )
              }
            />
          )}
        </div>
      )}

      <DialogoDeConfirmacion
        abierto={pedidoAEliminar !== null}
        titulo="Eliminar pedido"
        descripcion={
          pedidoAEliminar
            ? `El pedido ${pedidoAEliminar.numeroPedido} de ${pedidoAEliminar.cliente} dejará de aparecer en el listado. La eliminación es lógica: el registro se conserva en la base de datos.`
            : ''
        }
        textoDeConfirmacion="Eliminar"
        procesando={eliminando}
        onConfirmar={() => void confirmarEliminacion()}
        onCancelar={() => setPedidoAEliminar(null)}
      />
    </section>
  )
}
