# Web

Proyecto Blazor Web App. Es el punto de entrada HTTP y solo debe coordinar UI, configuracion y dependency injection.

La UI final se implementara en fases posteriores.

## Autenticacion

El login simple esta en `/login`. El seed local crea usuarios demo configurados en `appsettings.DevelopmentLocal.json`:

- Admin: `admin@regional.test` / `Admin123!`
- Moderator: `moderator@regional.test` / `Moderator123!`
- Contributor: `contributor@regional.test` / `Contributor123!`

El Contributor queda asignado a equipos seed mediante `ClubContributor.TeamId`; desde `/admin` solo ve y edita partidos donde participa alguno de esos equipos.

Para ZimaBoard no uses credenciales en `appsettings`. Configura `ConnectionStrings__DefaultConnection` y, si hace falta seed inicial, `Seed__AdminEmail`, `Seed__AdminDisplayName`, `Seed__AdminPassword`, `Seed__ModeratorEmail`, `Seed__ModeratorDisplayName`, `Seed__ModeratorPassword`, `Seed__ContributorEmail`, `Seed__ContributorDisplayName` y `Seed__ContributorPassword` como variables de entorno.
