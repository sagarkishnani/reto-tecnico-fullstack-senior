import { Link } from 'react-router-dom'
import { rutas } from '@/routes/rutas'

export function PaginaNoEncontrada() {
  return (
    <div className="flex min-h-dvh flex-col items-center justify-center gap-4 px-4 text-center">
      <p className="text-sm font-medium text-marca-600">Error 404</p>
      <h1 className="text-2xl font-semibold tracking-tight text-slate-900">Página no encontrada</h1>
      <p className="max-w-sm text-sm text-slate-600">
        La dirección que abriste no existe o el recurso fue movido.
      </p>
      <Link
        to={rutas.pedidos}
        className="rounded-md bg-marca-600 px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-marca-700"
      >
        Volver a pedidos
      </Link>
    </div>
  )
}
