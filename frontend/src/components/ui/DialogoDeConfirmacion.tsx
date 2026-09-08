import { useEffect, useRef } from 'react'
import { Boton } from '@/components/ui/Boton'

type DialogoDeConfirmacionProps = {
  abierto: boolean
  titulo: string
  descripcion: string
  textoDeConfirmacion: string
  procesando?: boolean
  onConfirmar: () => void
  onCancelar: () => void
}

export function DialogoDeConfirmacion({
  abierto,
  titulo,
  descripcion,
  textoDeConfirmacion,
  procesando = false,
  onConfirmar,
  onCancelar,
}: DialogoDeConfirmacionProps) {
  const dialogo = useRef<HTMLDialogElement>(null)

  useEffect(() => {
    const elemento = dialogo.current

    if (!elemento) {
      return
    }

    if (abierto && !elemento.open) {
      elemento.showModal()
    }

    if (!abierto && elemento.open) {
      elemento.close()
    }
  }, [abierto])

  return (
    <dialog
      ref={dialogo}
      onCancel={(evento) => {
        evento.preventDefault()
        if (!procesando) onCancelar()
      }}
      className="m-auto w-[min(28rem,calc(100vw-2rem))] rounded-xl border border-slate-200 p-0 shadow-xl backdrop:bg-slate-900/40"
    >
      <div className="space-y-2 p-6">
        <h2 className="text-base font-semibold text-slate-900">{titulo}</h2>
        <p className="text-sm text-slate-600">{descripcion}</p>
      </div>

      <div className="flex justify-end gap-2 border-t border-slate-100 bg-slate-50 px-6 py-4">
        <Boton variante="secundario" disabled={procesando} onClick={onCancelar}>
          Cancelar
        </Boton>

        <Boton variante="peligro" cargando={procesando} onClick={onConfirmar}>
          {textoDeConfirmacion}
        </Boton>
      </div>
    </dialog>
  )
}
