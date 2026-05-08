# Infrastructure

Contiene persistencia, configuracion de EF Core, integraciones tecnicas y servicios concretos.

Las migraciones EF deben generarse desde el proyecto Web usando `RegionalLeagueApp.Infrastructure` como assembly de migraciones.

## Seed de desarrollo

El seed se configura en `RegionalLeagueApp.Web/appsettings.Development.json`, seccion `Seed`.
El usuario admin inicial queda definido por `AdminEmail`, `AdminDisplayName` y `AdminPassword`.
El seed crea el usuario de ASP.NET Core Identity, le asigna el rol `Admin` y crea su perfil de dominio relacionado.
