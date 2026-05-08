namespace RegionalLeagueApp.Application.Standings;

public interface IStandingsRecalculationService
{
    Task RecalculateCompetitionAsync(Guid competitionId, CancellationToken cancellationToken = default);
}
