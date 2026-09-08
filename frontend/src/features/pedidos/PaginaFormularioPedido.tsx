import { useParams } from 'react-router-dom'
import { Marcador } from '@/components/Marcador'

export function PaginaFormularioPedido() {
  const { id } = useParams()

  return (
    <Marcador
      titulo={id ? `Editar pedido ${id}` : 'Nuevo pedido'}
      descripcion="El formulario llega en el paso 12."
    />
  )
}
