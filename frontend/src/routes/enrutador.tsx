import { createBrowserRouter, Navigate } from 'react-router-dom'
import { LayoutPrincipal } from '@/components/LayoutPrincipal'
import { PaginaNoEncontrada } from '@/components/PaginaNoEncontrada'
import { PaginaLogin } from '@/features/auth/PaginaLogin'
import { PaginaFormularioPedido } from '@/features/pedidos/PaginaFormularioPedido'
import { PaginaListaPedidos } from '@/features/pedidos/PaginaListaPedidos'
import { patrones, rutas } from '@/routes/rutas'

export const enrutador = createBrowserRouter([
  {
    path: rutas.login,
    element: <PaginaLogin />,
  },
  {
    path: '/',
    element: <LayoutPrincipal />,
    children: [
      { index: true, element: <Navigate to={rutas.pedidos} replace /> },
      { path: rutas.pedidos, element: <PaginaListaPedidos /> },
      { path: rutas.nuevoPedido, element: <PaginaFormularioPedido /> },
      { path: patrones.editarPedido, element: <PaginaFormularioPedido /> },
    ],
  },
  {
    path: '*',
    element: <PaginaNoEncontrada />,
  },
])
