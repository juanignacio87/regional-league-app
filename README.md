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
