import type { ReactNode } from 'react'

type EstadoVacioProps = {
  titulo: string
  descripcion: string
  accion?: ReactNode
}

export function EstadoVacio({ titulo, descripcion, accion }: EstadoVacioProps) {
  return (
    <div className="flex flex-col items-center gap-2 px-6 py-16 text-center">
      <svg className="size-10 text-slate-300" viewBox="0 0 24 24" fill="none" aria-hidden="true">
        <path
          d="M4 8.5 12 4l8 4.5v7L12 20l-8-4.5z"
          stroke="currentColor"
          strokeWidth="1.5"
          strokeLinejoin="round"
        />
        <path d="M4 8.5 12 13m0 0 8-4.5M12 13v7" stroke="currentColor" strokeWidth="1.5" />
      </svg>

      <h2 className="text-sm font-semibold text-slate-900">{titulo}</h2>
      <p className="max-w-sm text-sm text-slate-500">{descripcion}</p>

      {accion && <div className="mt-2">{accion}</div>}
    </div>
  )
}
