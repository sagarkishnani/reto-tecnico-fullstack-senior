# API de Pedidos — Reto Técnico Fullstack Senior

Solución end-to-end para la gestión de pedidos: API REST en .NET 9 con arquitectura limpia,
autenticación JWT, persistencia en PostgreSQL con Entity Framework Core, y cliente web en React.

---

## Tabla de contenidos

- [Arquitectura](#arquitectura)
- [Requisitos previos](#requisitos-previos)
- [Puesta en marcha](#puesta-en-marcha)
- [Credenciales de prueba](#credenciales-de-prueba)
- [Endpoints](#endpoints)
- [Reglas de negocio](#reglas-de-negocio)
- [Seguridad](#seguridad)
- [Resiliencia](#resiliencia)
- [Base de datos](#base-de-datos)
- [Tests](#tests)
- [Colección de Postman](#colección-de-postman)
- [Configuración](#configuración)
- [Decisiones técnicas](#decisiones-técnicas)

---

## Arquitectura

Cuatro capas con las dependencias apuntando siempre hacia el dominio:

```
Pedidos.Api ──────────> Pedidos.Application ──────> Pedidos.Domain
     └──────────────> Pedidos.Infrastructure ──────────┘
```

| Proyecto | Responsabilidad | Dependencias externas |
|---|---|---|
| `Pedidos.Domain` | Entidades, reglas de negocio, contratos de repositorio | **ninguna** |
| `Pedidos.Application` | Casos de uso, DTOs, puertos de seguridad y persistencia | contenedor de DI |
| `Pedidos.Infrastructure` | EF Core, PostgreSQL, BCrypt, JWT, Polly | EF Core, Npgsql, Polly |
| `Pedidos.Api` | Controllers, middleware, configuración | ASP.NET Core, Serilog |

`Pedidos.Domain` no referencia ningún paquete NuGet. Las reglas de negocio no saben que existe HTTP,
Entity Framework ni PostgreSQL.

```
.
├── backend/
│   ├── Pedidos.sln
│   ├── Directory.Build.props
│   ├── src/
│   │   ├── Pedidos.Domain/
│   │   │   ├── Common/            EntidadBase con auditoría y eliminación lógica
│   │   │   ├── Entities/          Pedido, Usuario
│   │   │   ├── Enums/             EstadoPedido, RolUsuario
│   │   │   ├── Exceptions/        jerarquía de DomainException
│   │   │   └── Repositories/      IPedidoRepository, IUsuarioRepository
│   │   ├── Pedidos.Application/
│   │   │   ├── Abstractions/      IUnitOfWork, IPasswordHasher, IGeneradorDeToken
│   │   │   ├── Autenticacion/     caso de uso de login
│   │   │   └── Pedidos/           casos de uso del CRUD, DTOs, mapeo
│   │   ├── Pedidos.Infrastructure/
│   │   │   ├── Persistence/       DbContext, configuraciones, repositorios, migraciones
│   │   │   ├── Resiliencia/       pipelines de Polly
│   │   │   └── Security/          BCrypt, generador de JWT
│   │   └── Pedidos.Api/
│   │       ├── Controllers/       AuthController, PedidosController
│   │       ├── Errores/           manejador global de excepciones
│   │       ├── Extensions/        JWT, CORS, rate limiting, Serilog, OpenAPI
│   │       └── OpenApi/           esquema de seguridad para Swagger
│   └── tests/
│       ├── Pedidos.Domain.Tests/
│       ├── Pedidos.Application.Tests/
│       └── Pedidos.Infrastructure.Tests/
├── database/
│   └── schema.sql                 script idempotente del esquema
├── postman/
│   └── Pedidos.postman_collection.json
└── frontend/
    ├── src/
    │   ├── api/               cliente HTTP y funciones por recurso
    │   ├── components/ui/     primitivos reutilizables
    │   ├── features/          auth y pedidos
    │   ├── lib/               JWT y formato
    │   ├── routes/            enrutado
    │   ├── stores/            sesión, pedidos, notificaciones
    │   └── types/             espejo de los DTOs
    └── README.md
```

---

## Requisitos previos

| Herramienta | Versión | Comprobación |
|---|---|---|
| .NET SDK | 9.0 | `dotnet --version` |
| PostgreSQL | 14 o superior | `psql --version` |
| Node.js | 20 o superior | `node -v` |

Si no tiene el SDK de .NET:

```bash
curl -sSL https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 9.0
export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$DOTNET_ROOT:$DOTNET_ROOT/tools:$PATH"
```

---

## Puesta en marcha

### 1. Crear la base de datos

```bash
psql -d postgres -c "CREATE ROLE pedidos_user LOGIN PASSWORD 'pedidos_pass';"
createdb -O pedidos_user pedidos_db
```

No hace falta ejecutar ningún script: **las migraciones se aplican solas al arrancar la API**, y si
las tablas están vacías se cargan datos semilla. Para crear el esquema manualmente, el script está en
`database/schema.sql`.

### 2. Levantar la API

```bash
cd backend
dotnet run --project src/Pedidos.Api
```

Queda disponible en `http://localhost:5080`:

| Recurso | URL |
|---|---|
| Swagger UI | http://localhost:5080/swagger |
| Documento OpenAPI | http://localhost:5080/openapi/v1.json |
| Health check | http://localhost:5080/health |

### 3. Levantar el cliente web

```bash
cd frontend
npm install
npm run dev
```

Queda en `http://localhost:5173`, que es el origen autorizado en la política de CORS. El detalle del
cliente está en [`frontend/README.md`](frontend/README.md).

| Pantalla | Ruta |
|---|---|
| Inicio de sesión | `/login` |
| Listado de pedidos | `/pedidos` |
| Crear pedido | `/pedidos/nuevo` |
| Editar pedido | `/pedidos/:id/editar` |

---

## Credenciales de prueba

Se crean automáticamente la primera vez que arranca la API.

| Email | Contraseña | Rol |
|---|---|---|
| `user@email.com` | `123456` | User |
| `admin@email.com` | `Admin123*` | Admin |

Las contraseñas se almacenan con BCrypt, factor de trabajo 12.

---

## Endpoints

### Autenticación

```
POST /auth/login
```

```json
{
  "email": "user@email.com",
  "password": "123456"
}
```

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 3600
}
```

### Pedidos

Todos requieren la cabecera `Authorization: Bearer {token}`.

| Método | Ruta | Respuesta correcta | Errores posibles |
|---|---|---|---|
| `GET` | `/api/pedidos` | `200` | `401` |
| `GET` | `/api/pedidos/{id}` | `200` | `401`, `404` |
| `POST` | `/api/pedidos` | `201` + `Location` | `400`, `401`, `409` |
| `PUT` | `/api/pedidos/{id}` | `200` | `400`, `401`, `404`, `409` |
| `DELETE` | `/api/pedidos/{id}` | `204` | `401`, `404` |

Modelo de pedido:

```json
{
  "id": 1,
  "numeroPedido": "PED-001",
  "cliente": "Juan Perez",
  "fecha": "2025-01-10T00:00:00Z",
  "total": 250.75,
  "estado": "Registrado"
}
```

Estados válidos: `Registrado`, `EnProceso`, `Enviado`, `Entregado`, `Cancelado`.

### Formato de errores

Todas las respuestas de error siguen `ProblemDetails` (RFC 7807):

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.2",
  "title": "Conflicto de unicidad",
  "status": 409,
  "detail": "Ya existe un pedido con el número PED-001.",
  "instance": "POST /api/pedidos",
  "traceId": "00-f4de998e6dc2dee5c5995988f8bd9f6e-7a0791299aafec54-00"
}
```

El `traceId` permite correlacionar el error que ve el cliente con la línea correspondiente del log.

| Código | Cuándo |
|---|---|
| `400` | Regla de negocio incumplida |
| `401` | Sin token, token inválido o credenciales incorrectas |
| `404` | El recurso no existe o fue eliminado lógicamente |
| `409` | El número de pedido ya está en uso |
| `429` | Se superó el límite de peticiones |
| `503` | La base de datos no responde y el circuito está abierto |

---

## Reglas de negocio

| Regla | Dónde se aplica |
|---|---|
| El total debe ser mayor que 0 | Entidad `Pedido`, al crear y al actualizar |
| El número de pedido es único | Caso de uso + índice único parcial en PostgreSQL |
| Solo usuarios autenticados acceden al CRUD | `[Authorize]` a nivel de controller |
| Eliminación lógica | `EntidadBase` + filtro global de consulta en EF Core |

Detalles del comportamiento:

- El número de pedido se normaliza a mayúsculas y sin espacios: `ped-001` y `PED-001` son el mismo.
- El total se redondea a dos decimales y no puede superar `99,999,999.99`, el rango de `DECIMAL(10,2)`.
- Un pedido eliminado no puede modificarse y no aparece en ninguna consulta.
- Tras eliminar un pedido, su número queda libre para reutilizarse.
- Un pedido nuevo nace siempre en estado `Registrado`.

---

## Seguridad

- **JWT Bearer** firmado con HMAC-SHA256. Claims emitidos: `sub`, `email`, `jti`, `name`, `role`,
  `nbf`, `exp`, `iss`, `aud`.
- **Expiración de una hora**, configurable. `ClockSkew` reducido a 30 segundos para que la
  caducidad sea real.
- **La clave se valida al arrancar**: si falta o mide menos de 32 bytes, la aplicación no levanta.
- **Contraseñas con BCrypt**, factor de trabajo 12.
- **El login no revela si un email existe**: credenciales incorrectas y usuario inexistente devuelven
  la misma respuesta, y en ambos casos se verifica un hash para que el tiempo de respuesta no delate
  qué correos están registrados.
- **Rate limiting**: 5 intentos de login por minuto y 100 peticiones por minuto en general,
  particionados por usuario autenticado o por IP.
- **CORS restringido** al origen del cliente web.

---

## Resiliencia

Dos pipelines de Polly con estrategias distintas según la operación:

| Pipeline | Estrategias | Motivo |
|---|---|---|
| Lectura | Retry (3 intentos, backoff exponencial con jitter) → Circuit Breaker → Timeout 10s | Una lectura es idempotente |
| Escritura | Circuit Breaker → Timeout 15s, sin reintentos | Un `INSERT` no es idempotente: reintentarlo podría duplicar el pedido |

El retry solo actúa sobre fallos transitorios (`NpgsqlException.IsTransient`): una caída de conexión
se reintenta, una violación de restricción no.

Cuando el circuito se abre, la API responde `503` de inmediato en lugar de esperar el timeout en cada
petición, lo que evita saturar una base de datos que ya está en problemas.

---

## Base de datos

```sql
CREATE TABLE "Pedidos" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "NumeroPedido" character varying(50) NOT NULL,
    "Cliente" character varying(150) NOT NULL,
    "Fecha" timestamp with time zone NOT NULL,
    "Total" numeric(10,2) NOT NULL,
    "Estado" character varying(50) NOT NULL,
    "FechaCreacion" timestamp with time zone NOT NULL,
    "FechaActualizacion" timestamp with time zone,
    "Eliminado" boolean NOT NULL,
    "FechaEliminacion" timestamp with time zone,
    CONSTRAINT "PK_Pedidos" PRIMARY KEY ("Id")
);

CREATE UNIQUE INDEX "IX_Pedidos_NumeroPedido_Activos"
    ON "Pedidos" ("NumeroPedido") WHERE ("Eliminado" = false);
```

El índice único es **parcial**. Con un `UNIQUE` corriente, un pedido borrado lógicamente seguiría
ocupando su número indefinidamente y el usuario recibiría un error de duplicado por un registro que
ya no puede ver.

### Migraciones

Se aplican automáticamente al arrancar. Para operarlas manualmente:

```bash
cd backend
dotnet tool install --global dotnet-ef --version 9.0.19

dotnet ef migrations add NombreDeLaMigracion --project src/Pedidos.Infrastructure --output-dir Persistence/Migrations
dotnet ef database update --project src/Pedidos.Infrastructure
dotnet ef migrations script --project src/Pedidos.Infrastructure --idempotent --output ../database/schema.sql
```

Un `IDesignTimeDbContextFactory` permite que estos comandos funcionen sin arrancar la aplicación web.
Toma la cadena de conexión de `ConnectionStrings__Postgres` si está definida.

---

## Tests

```bash
cd backend
dotnet test
```

| Proyecto | Tests | Cubre |
|---|---|---|
| `Pedidos.Domain.Tests` | 45 | Invariantes de las entidades y reglas de negocio |
| `Pedidos.Application.Tests` | 31 | Casos de uso, con dobles de prueba escritos a mano |
| `Pedidos.Infrastructure.Tests` | 8 | Pipelines de resiliencia: retry, circuit breaker, timeout |

En el cliente web:

```bash
cd frontend
npm test
```

| Área | Tests | Cubre |
|---|---|---|
| `lib`, `api`, `stores`, `features` | 89 | JWT, cliente HTTP, stores y validación del formulario |

Los tests de aplicación usan repositorios en memoria que replican el comportamiento real, incluido el
filtro de eliminación lógica, en lugar de verificar llamadas a un mock.

---

## Colección de Postman

`postman/Pedidos.postman_collection.json` incluye el login, el CRUD completo y los casos de error,
con aserciones automáticas.

El request `Login` guarda el token en una variable de colección, así que el resto de peticiones se
autentican solas. Para ejecutarla desde la línea de comandos:

```bash
npx newman run postman/Pedidos.postman_collection.json
```

---

## Configuración

Los valores por defecto están en `appsettings.json` y los de desarrollo en
`appsettings.Development.json`. Cualquiera se sobrescribe con variables de entorno.

| Variable | Descripción |
|---|---|
| `ConnectionStrings__Postgres` | Cadena de conexión a PostgreSQL |
| `Jwt__Key` | Clave de firma, mínimo 32 bytes |
| `Jwt__ExpiracionEnMinutos` | Vigencia del token, por defecto 60 |
| `Cors__OrigenesPermitidos__0` | Origen del cliente web |
| `RateLimiting__IntentosDeLoginPorVentana` | Intentos de login permitidos, por defecto 5 |
| `Resiliencia__ReintentosMaximos` | Reintentos en lecturas, por defecto 3 |

`appsettings.json` nunca contiene la cadena de conexión ni la clave JWT: quedan vacías y la
aplicación falla al arrancar con un mensaje explícito si no se proporcionan.
