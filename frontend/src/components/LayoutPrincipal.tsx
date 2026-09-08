import { NavLink, Outlet, useNavigate } from 'react-router-dom'
import { Boton } from '@/components/ui/Boton'
import { rutas } from '@/routes/rutas'
import { useSesion } from '@/stores/sesion'

const enlaces = [
  { a: rutas.pedidos, texto: 'Pedidos' },
  { a: rutas.nuevoPedido, texto: 'Nuevo pedido' },
]

export function LayoutPrincipal() {
  const usuario = useSesion((estado) => estado.usuario)
  const cerrarSesion = useSesion((estado) => estado.cerrarSesion)
  const navegar = useNavigate()

  const salir = () => {
    cerrarSesion()
    navegar(rutas.login, { replace: true })
  }

  return (
    <div className="min-h-dvh">
      <header className="border-b border-slate-200 bg-white">
        <div className="mx-auto flex max-w-6xl flex-wrap items-center gap-x-6 gap-y-3 px-4 py-3 sm:px-6">
          <NavLink to={rutas.pedidos} className="flex items-center gap-2">
            <img src="/favicon.svg" alt="" className="size-6" />
            <span className="text-base font-semibold tracking-tight text-slate-900">Pedidos</span>
          </NavLink>

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

          <div className="ml-auto flex items-center gap-3">
            {usuario && (
              <div className="text-right leading-tight">
                <p className="text-sm font-medium text-slate-900">{usuario.nombre}</p>
                <p className="text-xs text-slate-500">
                  {usuario.email}
                  <span className="ml-1.5 rounded-sm bg-slate-100 px-1.5 py-0.5 font-medium text-slate-600">
                    {usuario.rol}
                  </span>
                </p>
              </div>
            )}

            <Boton variante="secundario" tamano="pequeno" onClick={salir}>
              Cerrar sesión
            </Boton>
          </div>
        </div>
      </header>

      <main className="mx-auto max-w-6xl px-4 py-8 sm:px-6">
        <Outlet />
      </main>
    </div>
  )
}
