import { describe, expect, it } from 'vitest'
import { leerUsuarioDelToken, tokenVigente } from '@/lib/jwt'

function construirToken(carga: Record<string, unknown>): string {
  const codificar = (valor: unknown) => {
    const bytes = new TextEncoder().encode(JSON.stringify(valor))
    const binario = Array.from(bytes, (byte) => String.fromCharCode(byte)).join('')

    return btoa(binario).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '')
  }

  return `${codificar({ alg: 'HS256', typ: 'JWT' })}.${codificar(carga)}.firma`
}

const cargaValida = {
  sub: '1',
  email: 'user@email.com',
  name: 'Usuario Demo',
  role: 'User',
  exp: 2000000000,
}

describe('leerUsuarioDelToken', () => {
  it('extrae los claims de un token bien formado', () => {
    const usuario = leerUsuarioDelToken(construirToken(cargaValida))

    expect(usuario).toEqual({
      id: '1',
      email: 'user@email.com',
      nombre: 'Usuario Demo',
      rol: 'User',
      expiraEn: 2000000000 * 1000,
    })
  })

  it('reconoce el rol de administrador', () => {
    const usuario = leerUsuarioDelToken(construirToken({ ...cargaValida, role: 'Admin' }))

    expect(usuario?.rol).toBe('Admin')
  })

  it('degrada a User un rol desconocido en vez de confiar en el token', () => {
    const usuario = leerUsuarioDelToken(construirToken({ ...cargaValida, role: 'SuperAdmin' }))

    expect(usuario?.rol).toBe('User')
  })

  it('usa el email como nombre cuando el claim name no viene', () => {
    const usuario = leerUsuarioDelToken(construirToken({ ...cargaValida, name: undefined }))

    expect(usuario?.nombre).toBe('user@email.com')
  })

  it.each([
    ['una cadena vacia', ''],
    ['un texto sin puntos', 'no-es-un-token'],
    ['un token con dos segmentos', 'a.b'],
    ['una carga util que no es json', 'abc.###.def'],
  ])('devuelve null ante %s', (_caso, token) => {
    expect(leerUsuarioDelToken(token)).toBeNull()
  })

  it.each([
    ['falta sub', { ...cargaValida, sub: undefined }],
    ['falta email', { ...cargaValida, email: undefined }],
    ['falta exp', { ...cargaValida, exp: undefined }],
  ])('devuelve null cuando %s', (_caso, carga) => {
    expect(leerUsuarioDelToken(construirToken(carga))).toBeNull()
  })

  it('decodifica correctamente caracteres no ascii', () => {
    const usuario = leerUsuarioDelToken(construirToken({ ...cargaValida, name: 'Andrés Muñoz' }))

    expect(usuario?.nombre).toBe('Andrés Muñoz')
  })
})

describe('tokenVigente', () => {
  it('acepta un token cuya expiracion es futura', () => {
    const usuario = leerUsuarioDelToken(construirToken(cargaValida))

    expect(tokenVigente(usuario, 1000)).toBe(true)
  })

  it('rechaza un token expirado', () => {
    const usuario = leerUsuarioDelToken(construirToken({ ...cargaValida, exp: 1000 }))

    expect(tokenVigente(usuario, 2000 * 1000)).toBe(false)
  })

  it('rechaza la ausencia de usuario', () => {
    expect(tokenVigente(null)).toBe(false)
  })
})
