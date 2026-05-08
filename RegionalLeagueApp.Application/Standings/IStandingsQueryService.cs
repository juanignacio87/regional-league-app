namespace RegionalLeagueApp.Application.Standings;

public interface IStandingsQueryService
{
    Task<IReadOnlyList<StandingRowDto>> GetStandingsAsync(CancellationToken cancellationToken = default);
}
