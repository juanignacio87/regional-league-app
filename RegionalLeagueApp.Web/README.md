# Web

Proyecto Blazor Web App. Es el punto de entrada HTTP y solo debe coordinar UI, configuracion y dependency injection.

La UI final se implementara en fases posteriores.

## Autenticacion

El login simple esta en `/login`. El seed local crea el admin configurado en `appsettings.DevelopmentLocal.json`.

Para ZimaBoard no uses credenciales en `appsettings`. Configura `ConnectionStrings__DefaultConnection` y, si hace falta seed inicial, `Seed__AdminEmail`, `Seed__AdminDisplayName` y `Seed__AdminPassword` como variables de entorno.
