import { useState, type FormEvent } from 'react'
import { Navigate, useLocation, useNavigate } from 'react-router-dom'
import { Alerta } from '@/components/ui/Alerta'
import { Boton } from '@/components/ui/Boton'
import { CampoDeTexto } from '@/components/ui/CampoDeTexto'
import { tokenVigente } from '@/lib/jwt'
import { rutas } from '@/routes/rutas'
import { useSesion } from '@/stores/sesion'

type ErroresDelFormulario = {
  email?: string
  password?: string
}

const credencialesDeDemostracion = [
  { etiqueta: 'Usuario', email: 'user@email.com', password: '123456' },
  { etiqueta: 'Administrador', email: 'admin@email.com', password: 'Admin123*' },
]

function validar(email: string, password: string): ErroresDelFormulario {
  const errores: ErroresDelFormulario = {}

  if (!email.trim()) {
    errores.email = 'El correo es obligatorio.'
  } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.trim())) {
    errores.email = 'Escribe un correo con formato válido.'
  }

  if (!password) {
    errores.password = 'La contraseña es obligatoria.'
  }

  return errores
}

export function PaginaLogin() {
  const usuario = useSesion((estado) => estado.usuario)
  const autenticando = useSesion((estado) => estado.autenticando)
  const errorDelServidor = useSesion((estado) => estado.error)
  const iniciarSesion = useSesion((estado) => estado.iniciarSesion)
  const limpiarError = useSesion((estado) => estado.limpiarError)

  const navegar = useNavigate()
  const ubicacion = useLocation()

  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [errores, setErrores] = useState<ErroresDelFormulario>({})

  const destino = (ubicacion.state as { origen?: string } | null)?.origen ?? rutas.pedidos

  if (tokenVigente(usuario)) {
    return <Navigate to={destino} replace />
  }

  const alEnviar = async (evento: FormEvent<HTMLFormElement>) => {
    evento.preventDefault()

    const erroresDetectados = validar(email, password)
    setErrores(erroresDetectados)

    if (Object.keys(erroresDetectados).length > 0) {
      return
    }

    const correcto = await iniciarSesion({ email: email.trim(), password })

    if (correcto) {
      navegar(destino, { replace: true })
    }
  }

  const usarCredenciales = (credenciales: (typeof credencialesDeDemostracion)[number]) => {
    setEmail(credenciales.email)
    setPassword(credenciales.password)
    setErrores({})
    limpiarError()
  }

  return (
    <div className="flex min-h-dvh items-center justify-center bg-slate-100 px-4 py-10">
      <div className="w-full max-w-sm">
        <div className="mb-6 flex flex-col items-center text-center">
          <img src="/favicon.svg" alt="" className="size-11" />
          <h1 className="mt-3 text-xl font-semibold tracking-tight text-slate-900">
            Gestión de Pedidos
          </h1>
          <p className="mt-1 text-sm text-slate-600">Ingresa con tu cuenta para continuar</p>
        </div>

        <form
          onSubmit={alEnviar}
          noValidate
          className="space-y-4 rounded-xl border border-slate-200 bg-white p-6 shadow-sm"
        >
          {errorDelServidor && <Alerta tono="error">{errorDelServidor}</Alerta>}

          <CampoDeTexto
            etiqueta="Correo electrónico"
            type="email"
            name="email"
            autoComplete="username"
            placeholder="user@email.com"
            value={email}
            error={errores.email}
            disabled={autenticando}
            onChange={(evento) => {
              setEmail(evento.target.value)
              if (errorDelServidor) limpiarError()
            }}
          />

          <CampoDeTexto
            etiqueta="Contraseña"
            type="password"
            name="password"
            autoComplete="current-password"
            placeholder="••••••"
            value={password}
            error={errores.password}
            disabled={autenticando}
            onChange={(evento) => {
              setPassword(evento.target.value)
              if (errorDelServidor) limpiarError()
            }}
          />

          <Boton type="submit" cargando={autenticando} className="w-full">
            {autenticando ? 'Verificando' : 'Iniciar sesión'}
          </Boton>
        </form>

        <div className="mt-5 rounded-lg border border-dashed border-slate-300 bg-white/60 p-4">
          <p className="text-xs font-medium text-slate-500 uppercase">Cuentas de prueba</p>
          <div className="mt-2 flex flex-wrap gap-2">
            {credencialesDeDemostracion.map((credenciales) => (
              <Boton
                key={credenciales.email}
                variante="secundario"
                tamano="pequeno"
                disabled={autenticando}
                onClick={() => usarCredenciales(credenciales)}
              >
                {credenciales.etiqueta}
              </Boton>
            ))}
          </div>
        </div>
      </div>
    </div>
  )
}
