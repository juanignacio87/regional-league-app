namespace RegionalLeagueApp.Application.Scorers;

public interface IScorersQueryService
{
    Task<IReadOnlyList<ScorerRowDto>> GetScorersAsync(Guid? leagueId = null, Guid? competitionId = null, CancellationToken cancellationToken = default);
}
