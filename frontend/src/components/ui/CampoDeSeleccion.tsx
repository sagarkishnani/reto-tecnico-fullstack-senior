import { useId, type ReactNode, type SelectHTMLAttributes } from 'react'

type CampoDeSeleccionProps = SelectHTMLAttributes<HTMLSelectElement> & {
  etiqueta: string
  error?: string
  children: ReactNode
}

export function CampoDeSeleccion({
  etiqueta,
  error,
  className = '',
  id,
  children,
  ...resto
}: CampoDeSeleccionProps) {
  const idGenerado = useId()
  const idCampo = id ?? idGenerado
  const idError = `${idCampo}-error`

  return (
    <div className="space-y-1.5">
      <label htmlFor={idCampo} className="block text-sm font-medium text-slate-700">
        {etiqueta}
      </label>

      <select
        id={idCampo}
        aria-invalid={error ? true : undefined}
        aria-describedby={error ? idError : undefined}
        className={[
          'block w-full rounded-md border bg-white px-3 py-2 text-sm text-slate-900 shadow-xs transition-colors',
          'disabled:bg-slate-50 disabled:text-slate-500 focus:outline-none',
          error
            ? 'border-red-400 focus:border-red-500 focus:ring-2 focus:ring-red-100'
            : 'border-slate-300 focus:border-marca-500 focus:ring-2 focus:ring-marca-100',
          className,
        ].join(' ')}
        {...resto}
      >
        {children}
      </select>

      {error && (
        <p id={idError} className="text-xs font-medium text-red-600">
          {error}
        </p>
      )}
    </div>
  )
}
