import { peticion } from '@/api/cliente'
import type { LoginRequest, LoginResponse } from '@/types/autenticacion'

export function iniciarSesion(credenciales: LoginRequest): Promise<LoginResponse> {
  return peticion<LoginResponse>('/auth/login', {
    metodo: 'POST',
    cuerpo: credenciales,
    requiereAutenticacion: false,
  })
}
