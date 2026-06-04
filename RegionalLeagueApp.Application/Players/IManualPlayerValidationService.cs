namespace RegionalLeagueApp.Application.Players;

public interface IManualPlayerValidationService
{
    Task<string?> ValidateAsync(ManualPlayerValidationRequest request, CancellationToken cancellationToken = default);
}
