import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { tokenVigente } from '@/lib/jwt'
import { rutas } from '@/routes/rutas'
import { useSesion } from '@/stores/sesion'

export function RutaProtegida() {
  const usuario = useSesion((estado) => estado.usuario)
  const ubicacion = useLocation()

  if (!tokenVigente(usuario)) {
    return <Navigate to={rutas.login} state={{ origen: ubicacion.pathname }} replace />
  }

  return <Outlet />
}
