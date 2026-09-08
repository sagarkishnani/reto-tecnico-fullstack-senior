import type { ButtonHTMLAttributes, ReactNode } from 'react'

type VarianteBoton = 'primario' | 'secundario' | 'peligro' | 'fantasma'
type TamanoBoton = 'normal' | 'pequeno'

type BotonProps = ButtonHTMLAttributes<HTMLButtonElement> & {
  variante?: VarianteBoton
  tamano?: TamanoBoton
  cargando?: boolean
  children: ReactNode
}

const estilosBase =
  'inline-flex items-center justify-center gap-2 rounded-md font-medium transition-colors disabled:cursor-not-allowed disabled:opacity-60'

const porVariante: Record<VarianteBoton, string> = {
  primario: 'bg-marca-600 text-white hover:bg-marca-700',
  secundario: 'border border-slate-300 bg-white text-slate-700 hover:bg-slate-50',
  peligro: 'bg-red-600 text-white hover:bg-red-700',
  fantasma: 'text-slate-600 hover:bg-slate-100 hover:text-slate-900',
}

const porTamano: Record<TamanoBoton, string> = {
  normal: 'px-4 py-2 text-sm',
  pequeno: 'px-2.5 py-1.5 text-xs',
}

export function Boton({
  variante = 'primario',
  tamano = 'normal',
  cargando = false,
  disabled,
  className = '',
  children,
  ...resto
}: BotonProps) {
  return (
    <button
      type="button"
      disabled={disabled || cargando}
      aria-busy={cargando}
      className={[estilosBase, porVariante[variante], porTamano[tamano], className].join(' ')}
      {...resto}
    >
      {cargando && <Girador />}
      {children}
    </button>
  )
}

function Girador() {
  return (
    <svg className="size-4 animate-spin" viewBox="0 0 24 24" fill="none" aria-hidden="true">
      <circle cx="12" cy="12" r="9" stroke="currentColor" strokeWidth="3" className="opacity-25" />
      <path
        d="M21 12a9 9 0 0 0-9-9"
        stroke="currentColor"
        strokeWidth="3"
        strokeLinecap="round"
        className="opacity-90"
      />
    </svg>
  )
}
