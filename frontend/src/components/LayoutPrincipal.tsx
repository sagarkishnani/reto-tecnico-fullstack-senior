import { NavLink, Outlet } from 'react-router-dom'
import { rutas } from '@/routes/rutas'

const enlaces = [
  { a: rutas.pedidos, texto: 'Pedidos' },
  { a: rutas.nuevoPedido, texto: 'Nuevo pedido' },
]

export function LayoutPrincipal() {
  return (
    <div className="min-h-dvh">
      <header className="border-b border-slate-200 bg-white">
        <div className="mx-auto flex max-w-6xl items-center gap-6 px-4 py-3 sm:px-6">
          <span className="text-base font-semibold tracking-tight text-slate-900">Pedidos</span>

          <nav className="flex items-center gap-1">
            {enlaces.map((enlace) => (
              <NavLink
                key={enlace.a}
                to={enlace.a}
                end
                className={({ isActive }) =>
                  [
                    'rounded-md px-3 py-1.5 text-sm font-medium transition-colors',
                    isActive
                      ? 'bg-marca-50 text-marca-700'
                      : 'text-slate-600 hover:bg-slate-100 hover:text-slate-900',
                  ].join(' ')
                }
              >
                {enlace.texto}
              </NavLink>
            ))}
          </nav>
        </div>
      </header>

      <main className="mx-auto max-w-6xl px-4 py-8 sm:px-6">
        <Outlet />
      </main>
    </div>
  )
}
