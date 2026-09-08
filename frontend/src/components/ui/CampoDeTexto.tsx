import { useId, type InputHTMLAttributes } from 'react'

type CampoDeTextoProps = InputHTMLAttributes<HTMLInputElement> & {
  etiqueta: string
  error?: string
  ayuda?: string
}

export function CampoDeTexto({
  etiqueta,
  error,
  ayuda,
  className = '',
  id,
  ...resto
}: CampoDeTextoProps) {
  const idGenerado = useId()
  const idCampo = id ?? idGenerado
  const idError = `${idCampo}-error`
  const idAyuda = `${idCampo}-ayuda`

  return (
    <div className="space-y-1.5">
      <label htmlFor={idCampo} className="block text-sm font-medium text-slate-700">
        {etiqueta}
      </label>

      <input
        id={idCampo}
        aria-invalid={error ? true : undefined}
        aria-describedby={[error ? idError : null, ayuda ? idAyuda : null].filter(Boolean).join(' ') || undefined}
        className={[
          'block w-full rounded-md border bg-white px-3 py-2 text-sm text-slate-900 shadow-xs transition-colors',
          'placeholder:text-slate-400 disabled:bg-slate-50 disabled:text-slate-500',
          error
            ? 'border-red-400 focus:border-red-500 focus:ring-2 focus:ring-red-100'
            : 'border-slate-300 focus:border-marca-500 focus:ring-2 focus:ring-marca-100',
          'focus:outline-none',
          className,
        ].join(' ')}
        {...resto}
      />

      {ayuda && !error && (
        <p id={idAyuda} className="text-xs text-slate-500">
          {ayuda}
        </p>
      )}

      {error && (
        <p id={idError} className="text-xs font-medium text-red-600">
          {error}
        </p>
      )}
    </div>
  )
}
