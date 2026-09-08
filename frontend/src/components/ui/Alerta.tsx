import type { ReactNode } from 'react'

type TonoAlerta = 'error' | 'aviso' | 'exito' | 'informacion'

type AlertaProps = {
  tono?: TonoAlerta
  titulo?: string
  children: ReactNode
}

const porTono: Record<TonoAlerta, string> = {
  error: 'border-red-200 bg-red-50 text-red-800',
  aviso: 'border-amber-200 bg-amber-50 text-amber-800',
  exito: 'border-emerald-200 bg-emerald-50 text-emerald-800',
  informacion: 'border-slate-200 bg-slate-50 text-slate-700',
}

export function Alerta({ tono = 'error', titulo, children }: AlertaProps) {
  return (
    <div
      role={tono === 'error' ? 'alert' : 'status'}
      className={['rounded-md border px-3 py-2.5 text-sm', porTono[tono]].join(' ')}
    >
      {titulo && <p className="font-medium">{titulo}</p>}
      <div className={titulo ? 'mt-0.5' : undefined}>{children}</div>
    </div>
  )
}
