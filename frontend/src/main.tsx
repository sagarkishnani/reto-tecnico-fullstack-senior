import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { RouterProvider } from 'react-router-dom'
import { conectarSesion } from '@/app/conectarSesion'
import { enrutador } from '@/routes/enrutador'
import '@/index.css'

conectarSesion()

const contenedor = document.getElementById('root')

if (!contenedor) {
  throw new Error('No se encontró el elemento raíz de la aplicación.')
}

createRoot(contenedor).render(
  <StrictMode>
    <RouterProvider router={enrutador} />
  </StrictMode>,
)
