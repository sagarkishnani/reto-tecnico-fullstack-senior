import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import { iniciarSesion as iniciarSesionEnApi } from '@/api/autenticacion'
import { ErrorDeApi } from '@/api/errorDeApi'
import { leerUsuarioDelToken, tokenVigente } from '@/lib/jwt'
import type { LoginRequest, UsuarioAutenticado } from '@/types/autenticacion'

export const claveDeAlmacenamiento = 'pedidos.sesion'

type EstadoSesion = {
  token: string | null
  usuario: UsuarioAutenticado | null
  autenticando: boolean
  error: string | null
  iniciarSesion: (credenciales: LoginRequest) => Promise<boolean>
  cerrarSesion: () => void
  limpiarError: () => void
}

export const useSesion = create<EstadoSesion>()(
  persist(
    (set) => ({
      token: null,
      usuario: null,
      autenticando: false,
      error: null,

      iniciarSesion: async (credenciales) => {
        set({ autenticando: true, error: null })

        try {
          const { token } = await iniciarSesionEnApi(credenciales)
          const usuario = leerUsuarioDelToken(token)

          if (!tokenVigente(usuario)) {
            set({
              token: null,
              usuario: null,
              autenticando: false,
              error: 'El servidor devolvió un token inválido.',
            })

            return false
          }

          set({ token, usuario, autenticando: false, error: null })

          return true
        } catch (causa) {
          const error =
            causa instanceof ErrorDeApi
              ? causa.message
              : 'Ocurrió un error inesperado al iniciar sesión.'

          set({ token: null, usuario: null, autenticando: false, error })

          return false
        }
      },

      cerrarSesion: () => set({ token: null, usuario: null, error: null }),

      limpiarError: () => set({ error: null }),
    }),
    {
      name: claveDeAlmacenamiento,
      partialize: (estado) => ({ token: estado.token }),
      merge: (persistido, actual) => {
        const token = (persistido as { token?: string | null } | undefined)?.token ?? null
        const usuario = token ? leerUsuarioDelToken(token) : null

        return tokenVigente(usuario)
          ? { ...actual, token, usuario }
          : { ...actual, token: null, usuario: null }
      },
    },
  ),
)

export function haySesionActiva(): boolean {
  const { usuario } = useSesion.getState()

  return tokenVigente(usuario)
}
