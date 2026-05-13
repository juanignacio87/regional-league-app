namespace RegionalLeagueApp.Application.Standings;

public interface IStandingsQueryService
{
    Task<IReadOnlyList<StandingRowDto>> GetStandingsAsync(Guid? leagueId = null, CancellationToken cancellationToken = default);
}
