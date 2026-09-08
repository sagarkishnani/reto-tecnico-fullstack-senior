export type LoginRequest = {
  email: string
  password: string
}

export type LoginResponse = {
  token: string
  expiresIn: number
}

export type RolUsuario = 'User' | 'Admin'

export type UsuarioAutenticado = {
  id: string
  email: string
  nombre: string
  rol: RolUsuario
  expiraEn: number
}
