# RegionalLeagueApp

Base tecnica inicial para una aplicacion de liga regional de futbol con arquitectura monolitica modular.

## Proyectos

- `RegionalLeagueApp.Domain`: entidades y enums puros.
- `RegionalLeagueApp.Application`: contratos y casos de uso.
- `RegionalLeagueApp.Infrastructure`: EF Core, PostgreSQL y servicios tecnicos.
- `RegionalLeagueApp.Web`: Blazor Web App y composicion de la aplicacion.

## Referencias

- Application -> Domain
- Infrastructure -> Application, Domain
- Web -> Application, Infrastructure

## Build

```powershell
dotnet build RegionalLeagueApp.sln -m:1
```

`global.json` fija el SDK en .NET 8 para mantener la solucion alineada con el target `net8.0`.

## Deploy con Docker Compose

El deploy productivo simple usa:

- `Dockerfile`: build multi-stage para publicar `RegionalLeagueApp.Web` en una imagen ASP.NET Core 8 Alpine.
- `docker-compose.yml`: app web + PostgreSQL 16 + volumen de uploads.
- `.env`: variables reales locales al host. No se versiona.

### 1. Crear variables de entorno

```bash
cp .env.example .env
```

Editar `.env` y cambiar al menos:

```text
POSTGRES_PASSWORD=change_this_postgres_password
SEED_ADMIN_PASSWORD=ChangeThisAdmin123!
SEED_MODERATOR_PASSWORD=ChangeThisModerator123!
SEED_CONTRIBUTOR_PASSWORD=ChangeThisContributor123!
```

Para un primer deploy se puede dejar `SEED_ENABLED=true` para crear usuarios demo. Despues del primer arranque, cambiar a:

```text
SEED_ENABLED=false
```

### 2. Levantar servicios

```bash
docker compose up -d --build
```

La app queda disponible en:

```text
http://localhost:8080
```

Se puede cambiar el puerto con `WEB_PORT` en `.env`.

### 3. Migraciones

No hay comando manual separado: al iniciar, la app ejecuta `Database.MigrateAsync()` y aplica migraciones pendientes sobre PostgreSQL. Luego ejecuta el seed solo si `Seed__Enabled=true`.

### 4. Healthchecks

PostgreSQL usa:

```text
pg_isready
```

La web expone:

```text
/health
```

Ver estado:

```bash
docker compose ps
```

### 5. Uploads persistentes

Los logos subidos desde Admin se guardan en:

```text
/app/wwwroot/uploads
```

En Docker Compose quedan persistidos en el volumen:

```text
regional-league-app-uploads
```

### 6. Credenciales demo

Si `SEED_ENABLED=true`, se crean las cuentas definidas en `.env`:

- Admin: `SEED_ADMIN_EMAIL` / `SEED_ADMIN_PASSWORD`
- Moderator: `SEED_MODERATOR_EMAIL` / `SEED_MODERATOR_PASSWORD`
- Contributor: `SEED_CONTRIBUTOR_EMAIL` / `SEED_CONTRIBUTOR_PASSWORD`

Para produccion real, usar passwords fuertes, desactivar el seed despues del primer arranque y colocar la app detras de un reverse proxy con HTTPS.

## Entornos de base de datos

La app usa la connection string `ConnectionStrings:DefaultConnection`.
No hay credenciales reales hardcodeadas. En entornos remotos se debe configurar con la variable:

```text
ConnectionStrings__DefaultConnection
```

### DevelopmentLocal

Usa PostgreSQL local opcional via Docker. Levantar la base:

```powershell
docker compose -f compose.local.yaml up -d
```

Ejecutar la app con el entorno `DevelopmentLocal`:

```powershell
dotnet run --project RegionalLeagueApp.Web --launch-profile DevelopmentLocal
```

Este perfil usa `RegionalLeagueApp.Web/appsettings.DevelopmentLocal.json`, con credenciales solo de desarrollo local.

### ZimaBoard

En ZimaBoard configurar variables de entorno reales, por ejemplo:

```bash
export ASPNETCORE_ENVIRONMENT=ZimaBoard
export ConnectionStrings__DefaultConnection='Host=ZIMA_HOST_OR_IP;Port=5432;Database=DB_NAME;Username=DB_USER;Password=DB_PASSWORD;SSL Mode=Prefer;Trust Server Certificate=true'
export Seed__Enabled=false
```

Si se desea crear el admin inicial en ZimaBoard, habilitar temporalmente el seed y pasar credenciales por entorno:

```bash
export Seed__Enabled=true
export Seed__AdminEmail='admin@example.com'
export Seed__AdminDisplayName='Admin'
export Seed__AdminPassword='CHANGE_THIS_PASSWORD'
```

Despues del primer arranque, volver `Seed__Enabled=false` para evitar depender del seed en produccion.
