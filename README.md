# Regional League App

Plataforma web para gestionar ligas regionales de futbol: fixture, resultados, tabla de posiciones, clubes, planteles, perfiles de jugadores, eventos de partido y moderacion colaborativa.

El proyecto esta pensado como una aplicacion realista de portfolio: combina una experiencia publica tipo app deportiva con un panel administrativo protegido por roles, flujo de propuestas, persistencia PostgreSQL, actualizaciones en tiempo real y deploy con Docker.

## Screenshots

> Reemplazar estos placeholders por capturas reales del proyecto.

![Home](docs/screenshots/home.png)
![Match Center](docs/screenshots/match-center.png)
![Admin](docs/screenshots/admin.png)
![Standings](docs/screenshots/standings.png)
![Scorers](docs/screenshots/scorers.png)
![Player Profile](docs/screenshots/player-profile.png)

## Features Funcionales

- Fixture publico con partidos programados.
- Resultados y estados de partido.
- Tabla de posiciones calculada desde partidos finalizados.
- Ranking de goleadores basado en eventos reales.
- Clubes con logos, datos basicos y planteles.
- Perfiles publicos de jugadores.
- Match Center con marcador, timeline, eventos y estadisticas.
- Eventos de partido: goles, tarjetas amarillas y tarjetas rojas.
- Roles: Admin, Moderator y Contributor.
- Flujo de propuestas: contributors proponen cambios y moderators/admins aprueban o rechazan.
- Score derivado automaticamente desde eventos de gol.
- Actualizaciones en tiempo real con SignalR.

## Features Tecnicas

- .NET 8.
- Blazor Web App / Blazor Server interactivity.
- ASP.NET Core.
- ASP.NET Core Identity.
- Entity Framework Core.
- PostgreSQL.
- SignalR.
- Docker multi-stage build.
- Docker Compose con Postgres y volumen de uploads.
- Healthchecks para web y base de datos.
- Migraciones automaticas al arranque.
- Preparado para backups mediante volumen persistente de PostgreSQL.
- Arquitectura por capas dentro de un monolito modular.

## Arquitectura

La solucion mantiene una arquitectura simple por capas, sin microservicios:

```text
RegionalLeagueApp
|
+-- Domain
|   +-- Entidades, enums y reglas basicas del dominio
|
+-- Application
|   +-- Contratos, DTOs y servicios de aplicacion
|
+-- Infrastructure
|   +-- EF Core, PostgreSQL, Identity, seed y servicios tecnicos
|
+-- Web
    +-- Blazor UI, rutas, autenticacion, SignalR y composicion
```

Referencias:

```text
Application -> Domain
Infrastructure -> Application + Domain
Web -> Application + Infrastructure
```

La app es un monolito modular: evita la complejidad operativa de microservicios, pero separa responsabilidades para que el codigo sea mantenible.

## Flujos Destacados

### Colaboracion

1. Un Contributor entra al panel Admin.
2. Solo ve partidos donde tiene permisos.
3. Al cargar un resultado, estado o evento, se crea una propuesta pendiente.
4. Un Moderator o Admin revisa la propuesta.
5. Al aprobar, se aplica el cambio real y se notifica a las paginas publicas.

### Score Derivado

Los goles oficiales salen de `MatchEvents` tipo `Goal`.

`HomeScore` y `AwayScore` se recalculan automaticamente desde esos eventos para mantener consistencia entre:

- marcador
- timeline
- standings
- scorers
- Match Center

### Tiempo Real

Cuando Admin/Moderator aprueba cambios o modifica eventos, la app emite `MatchDataChanged` por SignalR. Las paginas publicas relevantes refrescan sus datos sin reload manual.

## Como Correr Localmente

Requisitos:

- .NET 8 SDK
- Docker Desktop o Docker Engine

Compilar:

```powershell
dotnet build .\RegionalLeagueApp.sln -m:1
```

### DevelopmentLocal

Levantar PostgreSQL local:

```powershell
docker compose -f compose.local.yaml up -d
```

Ejecutar la app:

```powershell
dotnet run --project .\RegionalLeagueApp.Web\RegionalLeagueApp.Web.csproj --launch-profile DevelopmentLocal
```

Este perfil usa `RegionalLeagueApp.Web/appsettings.DevelopmentLocal.json` con credenciales solo de desarrollo.

Las migraciones se aplican automaticamente al iniciar mediante `Database.MigrateAsync()`.

## Docker Compose

Crear un archivo `.env` desde la plantilla:

```bash
cp .env.example .env
```

Editar passwords y variables reales:

```text
POSTGRES_PASSWORD=change_this_postgres_password
SEED_ADMIN_PASSWORD=ChangeThisAdmin123!
SEED_MODERATOR_PASSWORD=ChangeThisModerator123!
SEED_CONTRIBUTOR_PASSWORD=ChangeThisContributor123!
```

Levantar app + PostgreSQL:

```bash
docker compose up -d --build
```

Acceso local:

```text
http://localhost:8080
```

Estado de servicios:

```bash
docker compose ps
```

Healthcheck web:

```text
http://localhost:8080/health
```

Uploads persistentes:

```text
regional-league-app-uploads -> /app/wwwroot/uploads
```

PostgreSQL persistente:

```text
regional-league-app-postgres -> /var/lib/postgresql/data
```

## Credenciales Demo

En `DevelopmentLocal`:

- Admin: `admin@regional.test` / `Admin123!`
- Moderator: `moderator@regional.test` / `Moderator123!`
- Contributor: `contributor@regional.test` / `Contributor123!`

En Docker Compose, las credenciales demo salen de `.env`:

- `SEED_ADMIN_EMAIL` / `SEED_ADMIN_PASSWORD`
- `SEED_MODERATOR_EMAIL` / `SEED_MODERATOR_PASSWORD`
- `SEED_CONTRIBUTOR_EMAIL` / `SEED_CONTRIBUTOR_PASSWORD`

Para un primer deploy se puede usar:

```text
SEED_ENABLED=true
```

Despues del primer arranque, cambiar a:

```text
SEED_ENABLED=false
```

## Configuracion

La connection string se lee desde:

```text
ConnectionStrings__DefaultConnection
```

Ejemplo para entorno remoto:

```bash
export ASPNETCORE_ENVIRONMENT=Production
export ConnectionStrings__DefaultConnection='Host=HOST;Port=5432;Database=DB;Username=USER;Password=PASSWORD'
export Seed__Enabled=false
```

## Roadmap

- Soporte multi-liga.
- Soporte multi-temporada avanzado.
- Panel especifico para administradores de club.
- Sponsors y banners por competencia/club.
- Suite de tests automatizados.
- CI/CD.
- HTTPS publico con reverse proxy.
- Backups programados documentados para PostgreSQL.

## Notas de Seguridad

- `.env` no se commitea.
- Las credenciales demo son solo para desarrollo.
- En produccion usar passwords fuertes y desactivar seed despues del primer arranque.
- Los uploads estan limitados por tipo y tamano.
- Los nombres de archivos subidos se generan con `Guid` para evitar sobrescrituras peligrosas.
- Para exposicion publica se recomienda usar HTTPS detras de un reverse proxy.
