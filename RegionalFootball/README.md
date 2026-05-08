# RegionalFootball

Aplicacion monolitica modular inspirada en OneFootball para una liga regional.

## Stack

- .NET 8
- Blazor Web App con renderizado interactivo server-side
- ASP.NET Core Identity con roles
- Entity Framework Core
- PostgreSQL
- Docker / Docker Compose

## Modulos

- `Modules/Identity`: usuarios y roles.
- `Modules/Competitions`: liga, temporada, competencia y fechas.
- `Modules/Clubs`: clubes, sedes y equipos.
- `Modules/Players`: jugadores.
- `Modules/Matches`: partidos y eventos.
- `Modules/Standings`: tabla de posiciones y recalculo.
- `Modules/Collaboration`: contributors, propuestas y auditoria.

## Roles

- Visitor: acceso publico de solo lectura.
- Contributor: carga propuestas de resultados.
- Moderator: aprueba o rechaza propuestas.
- Admin: panel administrativo y control total.

## Ejecutar con Docker

```powershell
docker compose up --build
```

Luego abrir:

```text
http://localhost:8080
```

Usuario seed de desarrollo:

```text
admin@regional.test
Admin123!
```

## Ejecutar local

Levantar PostgreSQL y usar la connection string de `appsettings.json`.

```powershell
dotnet run
```

La app aplica migraciones pendientes y carga datos iniciales al iniciar.

## Despliegue en ZimaBoard

1. Instalar Docker y Docker Compose en el ZimaBoard.
2. Copiar este directorio al servidor.
3. Cambiar `POSTGRES_PASSWORD` y `ConnectionStrings__DefaultConnection` en `compose.yaml`.
4. Ejecutar `docker compose up -d --build`.
5. Publicar el puerto `8080` mediante reverse proxy si se usa dominio propio.

## Decisiones

- Monolito modular: un despliegue, un proceso y un `ApplicationDbContext`, con carpetas por modulo para mantener limites claros sin complejidad de microservicios.
- Propuestas moderadas: los contributors no escriben resultados finales directamente; generan `MatchUpdateProposal` para validacion.
- Tabla recalculada desde partidos finalizados: evita inconsistencias manuales y mantiene una fuente de verdad simple.
