namespace RegionalLeagueApp.Application.Players;

public sealed record ManualPlayerValidationRequest(
    Guid TeamId,
    string DisplayName,
    int? ShirtNumber,
    bool IsActive,
    DateOnly? BirthDate,
    Guid? CurrentPlayerId = null);
