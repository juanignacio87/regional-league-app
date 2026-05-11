namespace RegionalLeagueApp.Application.Matches;

public interface IMatchScoreRecalculationService
{
    Task RecalculateMatchScoreAsync(Guid matchId, CancellationToken cancellationToken = default);
    Task RecalculateMatchScoreAndStandingsAsync(Guid matchId, CancellationToken cancellationToken = default);
}
