import type { RolUsuario, UsuarioAutenticado } from '@/types/autenticacion'

type CargaUtilJwt = {
  sub?: string
  email?: string
  name?: string
  role?: string
  exp?: number
}

const rolesValidos: RolUsuario[] = ['User', 'Admin']

function decodificarBase64Url(segmento: string): string {
  const base64 = segmento.replace(/-/g, '+').replace(/_/g, '/')
  const relleno = base64.padEnd(base64.length + ((4 - (base64.length % 4)) % 4), '=')
  const binario = atob(relleno)
  const bytes = Uint8Array.from(binario, (caracter) => caracter.charCodeAt(0))

  return new TextDecoder().decode(bytes)
}

export function leerUsuarioDelToken(token: string): UsuarioAutenticado | null {
  const segmentos = token.split('.')

  if (segmentos.length !== 3) {
    return null
  }

  let carga: CargaUtilJwt

  try {
    carga = JSON.parse(decodificarBase64Url(segmentos[1])) as CargaUtilJwt
  } catch {
    return null
  }

  if (!carga.sub || !carga.email || typeof carga.exp !== 'number') {
    return null
  }

  const rol = rolesValidos.includes(carga.role as RolUsuario) ? (carga.role as RolUsuario) : 'User'

  return {
    id: carga.sub,
    email: carga.email,
    nombre: carga.name ?? carga.email,
    rol,
    expiraEn: carga.exp * 1000,
  }
}

export function tokenVigente(usuario: UsuarioAutenticado | null, ahora = Date.now()): boolean {
  return usuario !== null && usuario.expiraEn > ahora
}
