import { useEffect, useState, type FormEvent } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { ErrorDeApi } from '@/api/errorDeApi'
import { obtenerPedidoPorId } from '@/api/pedidos'
import { Alerta } from '@/components/ui/Alerta'
import { Boton } from '@/components/ui/Boton'
import { CampoDeSeleccion } from '@/components/ui/CampoDeSeleccion'
import { CampoDeTexto } from '@/components/ui/CampoDeTexto'
import { EstadoVacio } from '@/components/ui/EstadoVacio'
import {
  hayErrores,
  interpretarTotal,
  limites,
  normalizarNumeroPedido,
  validarBorrador,
  type BorradorDePedido,
  type ErroresDePedido,
} from '@/features/pedidos/validacionDePedido'
import { etiquetaDeEstado } from '@/features/pedidos/etiquetasDeEstado'
import { aFechaDeFormulario } from '@/lib/formato'
import { rutas } from '@/routes/rutas'
import { notificarExito } from '@/stores/notificaciones'
import { usePedidos } from '@/stores/pedidos'
import { estadosDePedido, type EstadoPedido, type Pedido } from '@/types/pedido'

const borradorInicial: BorradorDePedido = {
  numeroPedido: '',
  cliente: '',
  fecha: new Date().toISOString().slice(0, 10),
  total: '',
  estado: 'Registrado',
}

function aBorrador(pedido: Pedido): BorradorDePedido {
  return {
    numeroPedido: pedido.numeroPedido,
    cliente: pedido.cliente,
    fecha: aFechaDeFormulario(pedido.fecha),
    total: pedido.total.toFixed(2),
    estado: pedido.estado,
  }
}

export function PaginaFormularioPedido() {
  const { id } = useParams()
  const navegar = useNavigate()

  const esEdicion = id !== undefined
  const idNumerico = Number(id)

  const crear = usePedidos((estado) => estado.crear)
  const actualizar = usePedidos((estado) => estado.actualizar)
  const pedidoEnMemoria = usePedidos((estado) =>
    esEdicion ? estado.pedidos.find((pedido) => pedido.id === idNumerico) : undefined,
  )

  const [borrador, setBorrador] = useState<BorradorDePedido>(borradorInicial)
  const [errores, setErrores] = useState<ErroresDePedido>({})
  const [errorGeneral, setErrorGeneral] = useState<string | null>(null)
  const [guardando, setGuardando] = useState(false)
  const [cargando, setCargando] = useState(esEdicion)
  const [noEncontrado, setNoEncontrado] = useState(false)

  useEffect(() => {
    if (!esEdicion) {
      return
    }

    if (Number.isNaN(idNumerico)) {
      setNoEncontrado(true)
      setCargando(false)
      return
    }

    if (pedidoEnMemoria) {
      setBorrador(aBorrador(pedidoEnMemoria))
      setCargando(false)
      return
    }

    const controlador = new AbortController()

    obtenerPedidoPorId(idNumerico, controlador.signal)
      .then((pedido) => {
        setBorrador(aBorrador(pedido))
        setCargando(false)
      })
      .catch((causa: unknown) => {
        if (causa instanceof DOMException && causa.name === 'AbortError') {
          return
        }

        if (causa instanceof ErrorDeApi && causa.esNoEncontrado) {
          setNoEncontrado(true)
        } else {
          setErrorGeneral(
            causa instanceof ErrorDeApi ? causa.message : 'No se pudo cargar el pedido.',
          )
        }

        setCargando(false)
      })

    return () => controlador.abort()
  }, [esEdicion, idNumerico, pedidoEnMemoria])

  const cambiar = <Campo extends keyof BorradorDePedido>(
    campo: Campo,
    valor: BorradorDePedido[Campo],
  ) => {
    setBorrador((actual) => ({ ...actual, [campo]: valor }))
    setErrores((actuales) => ({ ...actuales, [campo]: undefined }))
    setErrorGeneral(null)
  }

  const enviar = async (evento: FormEvent<HTMLFormElement>) => {
    evento.preventDefault()

    const erroresDetectados = validarBorrador(borrador, esEdicion)
    setErrores(erroresDetectados)

    if (hayErrores(erroresDetectados)) {
      return
    }

    const datos = {
      numeroPedido: normalizarNumeroPedido(borrador.numeroPedido),
      cliente: borrador.cliente.trim(),
      fecha: `${borrador.fecha}T00:00:00Z`,
      total: interpretarTotal(borrador.total) ?? 0,
    }

    setGuardando(true)
    setErrorGeneral(null)

    try {
      if (esEdicion) {
        await actualizar(idNumerico, { ...datos, estado: borrador.estado })
      } else {
        await crear(datos)
      }

      notificarExito(
        esEdicion
          ? `Pedido ${datos.numeroPedido} actualizado.`
          : `Pedido ${datos.numeroPedido} creado.`,
      )

      navegar(rutas.pedidos, { replace: true })
    } catch (causa) {
      if (causa instanceof ErrorDeApi && causa.esConflicto) {
        setErrores({ numeroPedido: causa.message })
      } else if (causa instanceof ErrorDeApi && causa.esNoEncontrado) {
        setNoEncontrado(true)
      } else {
        setErrorGeneral(
          causa instanceof ErrorDeApi ? causa.message : 'No se pudo guardar el pedido.',
        )
      }

      setGuardando(false)
    }
  }

  if (noEncontrado) {
    return (
      <div className="rounded-lg border border-slate-200 bg-white">
        <EstadoVacio
          titulo="El pedido no existe"
          descripcion="Puede que lo hayan eliminado o que el enlace sea incorrecto."
          accion={
            <Link
              to={rutas.pedidos}
              className="inline-flex items-center rounded-md bg-marca-600 px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-marca-700"
            >
              Volver a pedidos
            </Link>
          }
        />
      </div>
    )
  }

  return (
    <section className="mx-auto max-w-2xl space-y-5">
      <header>
        <h1 className="text-xl font-semibold tracking-tight text-slate-900">
          {esEdicion ? 'Editar pedido' : 'Nuevo pedido'}
        </h1>
        <p className="mt-0.5 text-sm text-slate-600">
          {esEdicion
            ? 'Modifica los datos del pedido y guarda los cambios.'
            : 'Registra un pedido nuevo. Nacerá en estado Registrado.'}
        </p>
      </header>

      {cargando ? (
        <div className="space-y-4 rounded-lg border border-slate-200 bg-white p-6">
          {Array.from({ length: 4 }, (_, indice) => (
            <div key={indice} className="space-y-1.5">
              <div className="h-3 w-24 animate-pulse rounded bg-slate-200" />
              <div className="h-9 animate-pulse rounded bg-slate-100" />
            </div>
          ))}
        </div>
      ) : (
        <form
          onSubmit={enviar}
          noValidate
          className="space-y-4 rounded-lg border border-slate-200 bg-white p-6 shadow-xs"
        >
          {errorGeneral && <Alerta tono="error">{errorGeneral}</Alerta>}

          <CampoDeTexto
            etiqueta="Número de pedido"
            name="numeroPedido"
            placeholder="PED-001"
            maxLength={limites.longitudNumeroPedido}
            value={borrador.numeroPedido}
            error={errores.numeroPedido}
            ayuda="Se guarda en mayúsculas y debe ser único."
            disabled={guardando}
            onChange={(evento) => cambiar('numeroPedido', evento.target.value)}
          />

          <CampoDeTexto
            etiqueta="Cliente"
            name="cliente"
            placeholder="Juan Perez"
            maxLength={limites.longitudCliente}
            value={borrador.cliente}
            error={errores.cliente}
            disabled={guardando}
            onChange={(evento) => cambiar('cliente', evento.target.value)}
          />

          <div className="grid gap-4 sm:grid-cols-2">
            <CampoDeTexto
              etiqueta="Fecha"
              name="fecha"
              type="date"
              value={borrador.fecha}
              error={errores.fecha}
              disabled={guardando}
              onChange={(evento) => cambiar('fecha', evento.target.value)}
            />

            <CampoDeTexto
              etiqueta="Total"
              name="total"
              inputMode="decimal"
              placeholder="250.75"
              value={borrador.total}
              error={errores.total}
              ayuda="Debe ser mayor que cero."
              disabled={guardando}
              onChange={(evento) => cambiar('total', evento.target.value)}
            />
          </div>

          {esEdicion && (
            <CampoDeSeleccion
              etiqueta="Estado"
              name="estado"
              value={borrador.estado}
              error={errores.estado}
              disabled={guardando}
              onChange={(evento) => cambiar('estado', evento.target.value as EstadoPedido)}
            >
              {estadosDePedido.map((estado) => (
                <option key={estado} value={estado}>
                  {etiquetaDeEstado(estado)}
                </option>
              ))}
            </CampoDeSeleccion>
          )}

          <div className="flex items-center justify-end gap-2 border-t border-slate-100 pt-4">
            <Link
              to={rutas.pedidos}
              className="rounded-md px-4 py-2 text-sm font-medium text-slate-600 transition-colors hover:bg-slate-100"
            >
              Cancelar
            </Link>

            <Boton type="submit" cargando={guardando}>
              {esEdicion ? 'Guardar cambios' : 'Crear pedido'}
            </Boton>
          </div>
        </form>
      )}
    </section>
  )
}
