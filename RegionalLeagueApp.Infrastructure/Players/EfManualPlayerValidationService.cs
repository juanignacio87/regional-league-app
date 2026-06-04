using Microsoft.EntityFrameworkCore;
using RegionalLeagueApp.Application.Players;
using RegionalLeagueApp.Infrastructure.Persistence;

namespace RegionalLeagueApp.Infrastructure.Players;

public sealed class EfManualPlayerValidationService(ApplicationDbContext dbContext) : IManualPlayerValidationService
{
    public async Task<string?> ValidateAsync(ManualPlayerValidationRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            return "El display name es obligatorio.";
        }

        if (request.ShirtNumber is <= 0)
        {
            return "El dorsal debe ser un numero positivo.";
        }

        if (request.BirthDate is not null && request.BirthDate > DateOnly.FromDateTime(DateTime.Today))
        {
            return "La fecha de nacimiento no puede estar en el futuro.";
        }

        if (!request.IsActive)
        {
            return null;
        }

        var activePlayers = await dbContext.Players
            .AsNoTracking()
            .Where(player =>
                player.TeamId == request.TeamId &&
                player.IsActive &&
                (request.CurrentPlayerId == null || player.Id != request.CurrentPlayerId.Value))
            .Select(player => new ExistingPlayer(player.DisplayName, player.ShirtNumber))
            .ToListAsync(cancellationToken);

        if (activePlayers.Any(player => Normalize(player.DisplayName) == Normalize(request.DisplayName)))
        {
            return "Ya existe un jugador activo con ese display name en este equipo.";
        }

        if (request.ShirtNumber is not null && activePlayers.Any(player => player.ShirtNumber == request.ShirtNumber))
        {
            return "Ya existe un jugador activo con ese dorsal en este equipo.";
        }

        return null;
    }

    private static string Normalize(string value) =>
        string.Join(' ', value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)).ToUpperInvariant();

    private sealed record ExistingPlayer(string DisplayName, int? ShirtNumber);
}
