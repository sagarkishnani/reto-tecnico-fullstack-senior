# Cliente web — Gestión de Pedidos

SPA en React 19 que consume la API REST de pedidos. Autenticación con JWT, CRUD completo y
validación que refleja las reglas de negocio del backend.

---

## Stack

| Herramienta | Versión | Rol |
|---|---|---|
| React | 19.2 | Interfaz |
| React Router | 7 | Enrutado y protección de rutas |
| Zustand | 5 | Estado de sesión, pedidos y notificaciones |
| Tailwind CSS | 4 | Estilos, configurados en CSS con `@theme` |
| TypeScript | 6 | Tipado estricto |
| Vite | 8 | Servidor de desarrollo y empaquetado |
| Vitest | 5 | Tests unitarios |

---

## Puesta en marcha

La API debe estar corriendo en `http://localhost:5080`.

```bash
npm install
npm run dev
```

Disponible en `http://localhost:5173`, que es el origen autorizado en la política de CORS del
backend. El puerto está fijado con `strictPort`: si está ocupado, el arranque falla en lugar de
saltar a otro puerto y provocar un error de CORS difícil de diagnosticar.

| Comando | Qué hace |
|---|---|
| `npm run dev` | Servidor de desarrollo |
| `npm run build` | Verificación de tipos y compilación a `dist/` |
| `npm run preview` | Sirve la compilación de producción |
| `npm test` | Tests unitarios |
| `npm run lint` | Análisis estático |

### Configuración

| Variable | Valor por defecto | Descripción |
|---|---|---|
| `VITE_API_URL` | `http://localhost:5080` | URL base de la API |

`.env.development` está versionado a propósito: solo contiene la URL de la API, que no es un secreto,
y permite que el proyecto funcione sin configuración previa.

---

## Credenciales de prueba

La pantalla de acceso incluye botones que rellenan estas credenciales con un clic.

| Email | Contraseña | Rol |
|---|---|---|
| `user@email.com` | `123456` | User |
| `admin@email.com` | `Admin123*` | Admin |

---

## Estructura

```
src/
├── api/              cliente HTTP y funciones por recurso
├── app/              cableado de la raíz de composición
├── components/
│   ├── ui/           primitivos reutilizables
│   └── ...           layout y página de error
├── features/
│   ├── auth/         login y protección de rutas
│   └── pedidos/      listado, formulario y validación
├── lib/              utilidades de JWT y formato
├── routes/           definición de rutas
├── stores/           sesión, pedidos y notificaciones
└── types/            tipos espejo de los DTOs de la API
```

### Pantallas

| Ruta | Pantalla |
|---|---|
| `/login` | Inicio de sesión |
| `/pedidos` | Listado con búsqueda, filtro por estado y eliminación |
| `/pedidos/nuevo` | Alta de pedido |
| `/pedidos/:id/editar` | Edición de pedido |

Todas menos `/login` requieren sesión activa.

---

## Decisiones técnicas

### Manejo de sesión

El token JWT se guarda en `localStorage` mediante el middleware `persist` de Zustand, y **solo se
persiste el token**: el usuario, su rol y la expiración se derivan del propio token al rehidratar.
Una única fuente de verdad, imposible de desincronizar.

Al rehidratar se valida la expiración: un token vencido arranca la sesión vacía en lugar de dejar la
aplicación en un estado que falla en la primera petición.

**Sobre el riesgo de `localStorage`:** es vulnerable a XSS, porque cualquier script inyectado puede
leerlo. Se eligió por ser lo habitual en una SPA con tokens Bearer y porque mantiene la sesión al
recargar. La alternativa robusta serían cookies `httpOnly` con tokens de refresco y rotación, que
requiere cambiar el contrato del backend y excede el alcance de este reto. Queda documentado como un
compromiso conocido, no como un descuido.

### Cliente HTTP

Un envoltorio propio sobre `fetch`, sin axios. Inyecta el token, normaliza los `ProblemDetails` que
devuelve la API en un `ErrorDeApi` tipado, y cierra la sesión cuando recibe un 401 en una petición
autenticada. Un 401 del propio login no cierra sesión, para no provocar un bucle de redirecciones.

El cliente **no importa el store**: recibe un proveedor de token que se cablea en `main.tsx`. Sin
esa inversión habría una dependencia circular entre el cliente y la sesión.

### Validación

La validación del cliente refleja las reglas del dominio del backend (total mayor que cero, tope de
`DECIMAL(10,2)`, longitudes máximas, normalización del número a mayúsculas), pero **no las
sustituye**: la decisión final siempre es del servidor.

La unicidad del número de pedido es la excepción deliberada: no se puede saber sin consultar la base
de datos. Cuando el servidor responde `409`, el mensaje se muestra en el campo del número, no en una
alerta genérica.

### Estado del servidor

Se usa Zustand también para los datos remotos, con `cargando`, `cargados` y `error` explícitos. Las
peticiones se cancelan con `AbortController` al desmontar el componente, y el store ignora el
`AbortError` para no mostrar errores fantasma al navegar rápido.

Un fallo de red **no** cierra la sesión: solo un 401 lo hace. Una caída momentánea de la API muestra
un aviso con botón de reintentar y conserva al usuario dentro.

### Fechas

La API devuelve marcas de tiempo en UTC. Formatearlas en la zona local desplazaría el día hacia atrás
en husos negativos, así que el formateador fija `timeZone: 'UTC'`. Hay tests que lo cubren.

### Accesibilidad

Etiquetas asociadas a cada campo, `aria-invalid` y `aria-describedby` en los errores, `aria-live` en
las notificaciones, y el diálogo de confirmación usa el elemento nativo `<dialog>` en modo modal, que
aporta atrapado de foco y cierre con Escape sin código adicional. El foco inicial del diálogo está en
"Cancelar", nunca en la acción destructiva.

---

## Tests

```bash
npm test
```

Cubren el decodificador de JWT (incluido un token real emitido por la API), el cliente HTTP con
`fetch` simulado, los tres stores y la validación del formulario.
