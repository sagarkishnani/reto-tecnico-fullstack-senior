import { conectarClienteConLaSesion } from '@/api/cliente'
import { useSesion } from '@/stores/sesion'

export function conectarSesion(): void {
  conectarClienteConLaSesion({
    obtenerToken: () => useSesion.getState().token,
    alExpirarSesion: () => useSesion.getState().cerrarSesion(),
  })
}
