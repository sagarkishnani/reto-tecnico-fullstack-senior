import { useEffect } from 'react'
import {
  useNotificaciones,
  type Notificacion,
  type TonoDeNotificacion,
} from '@/stores/notificaciones'

const duracionEnMilisegundos = 5000

const porTono: Record<TonoDeNotificacion, string> = {
  exito: 'border-emerald-200 bg-emerald-50 text-emerald-900',
  error: 'border-red-200 bg-red-50 text-red-900',
  informacion: 'border-slate-200 bg-white text-slate-800',
}

export function Notificaciones() {
  const notificaciones = useNotificaciones((estado) => estado.notificaciones)

  return (
    <div
      aria-live="polite"
      className="pointer-events-none fixed inset-x-0 bottom-0 z-50 flex flex-col items-center gap-2 p-4 sm:items-end"
    >
      {notificaciones.map((notificacion) => (
        <Aviso key={notificacion.id} notificacion={notificacion} />
      ))}
    </div>
  )
}

function Aviso({ notificacion }: { notificacion: Notificacion }) {
  const descartar = useNotificaciones((estado) => estado.descartar)

  useEffect(() => {
    const temporizador = setTimeout(() => descartar(notificacion.id), duracionEnMilisegundos)

    return () => clearTimeout(temporizador)
  }, [descartar, notificacion.id])

  return (
    <div
      role="status"
      className={[
        'pointer-events-auto flex w-full max-w-sm items-start gap-3 rounded-lg border px-4 py-3 text-sm shadow-lg',
        porTono[notificacion.tono],
      ].join(' ')}
    >
      <p className="flex-1">{notificacion.mensaje}</p>

      <button
        type="button"
        onClick={() => descartar(notificacion.id)}
        aria-label="Descartar notificación"
        className="-m-1 rounded p-1 opacity-60 transition-opacity hover:opacity-100"
      >
        <svg className="size-4" viewBox="0 0 20 20" fill="none" aria-hidden="true">
          <path d="m5 5 10 10M15 5 5 15" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" />
        </svg>
      </button>
    </div>
  )
}
