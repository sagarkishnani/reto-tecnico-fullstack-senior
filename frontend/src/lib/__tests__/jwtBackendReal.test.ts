import { describe, expect, it } from 'vitest'
import { leerUsuarioDelToken, tokenVigente } from '@/lib/jwt'

const tokenEmitidoPorLaApi =
  'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIyIiwiZW1haWwiOiJhZG1pbkBlbWFpbC5jb20iLCJqdGkiOiJkOGVhZDUzNi1mMGYzLTRkNTYtYWJlOC01Y2ZhMDVlMjBlMjEiLCJuYW1lIjoiQWRtaW5pc3RyYWRvciIsInJvbGUiOiJBZG1pbiIsIm5iZiI6MTc4ODg1MTI5MCwiZXhwIjoxNzg4ODU0ODkwLCJpc3MiOiJQZWRpZG9zLkFwaSIsImF1ZCI6IlBlZGlkb3MuQ2xpZW50In0.-oXu5cl-Tm6C2ZWZr2Nk-pOLwxupj8R1kdQ_6jJrrLY'

describe('interoperabilidad con el token que emite Pedidos.Api', () => {
  it('lee los claims tal como los escribe el backend', () => {
    const usuario = leerUsuarioDelToken(tokenEmitidoPorLaApi)

    expect(usuario).not.toBeNull()
    expect(usuario?.id).toBe('2')
    expect(usuario?.email).toBe('admin@email.com')
    expect(usuario?.nombre).toBe('Administrador')
    expect(usuario?.rol).toBe('Admin')
  })

  it('convierte la expiracion de segundos a milisegundos', () => {
    const usuario = leerUsuarioDelToken(tokenEmitidoPorLaApi)

    expect(usuario?.expiraEn).toBe(1788854890 * 1000)
    expect(tokenVigente(usuario, 1788851290 * 1000)).toBe(true)
    expect(tokenVigente(usuario, 1788854891 * 1000)).toBe(false)
  })
})
