namespace RegionalLeagueApp.Application.Discipline;

public interface IDisciplineQueryService
{
    Task<IReadOnlyList<DisciplineRowDto>> GetDisciplineAsync(Guid? leagueId = null, CancellationToken cancellationToken = default);

    Task<DisciplineRowDto?> GetPlayerDisciplineForMatchAsync(Guid matchId, Guid playerId, CancellationToken cancellationToken = default);
}
