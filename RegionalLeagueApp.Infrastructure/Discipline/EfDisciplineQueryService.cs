using Microsoft.EntityFrameworkCore;
using RegionalLeagueApp.Application.Discipline;
using RegionalLeagueApp.Domain.Matches;
using RegionalLeagueApp.Infrastructure.Persistence;

namespace RegionalLeagueApp.Infrastructure.Discipline;

public sealed class EfDisciplineQueryService(ApplicationDbContext dbContext) : IDisciplineQueryService
{
    public async Task<IReadOnlyList<DisciplineRowDto>> GetDisciplineAsync(Guid? leagueId = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.MatchEvents
            .AsNoTracking()
            .Where(matchEvent =>
                matchEvent.PlayerId != null &&
                (matchEvent.Type == MatchEventType.YellowCard || matchEvent.Type == MatchEventType.RedCard) &&
                matchEvent.Match!.Competition!.IsActive &&
                matchEvent.Match.Competition.Season!.League!.IsActive &&
                matchEvent.Team!.IsActive &&
                matchEvent.Team.Club!.IsActive &&
                matchEvent.Player!.IsActive);

        if (leagueId is not null)
        {
            query = query.Where(matchEvent => matchEvent.Match!.Competition!.Season!.LeagueId == leagueId);
        }

        var cardEvents = await query
            .Select(matchEvent => new CardEvent(
                matchEvent.MatchId,
                matchEvent.Match!.CompetitionId,
                matchEvent.Match.Competition!.Name,
                matchEvent.PlayerId!.Value,
                matchEvent.PlayerName != string.Empty ? matchEvent.PlayerName : matchEvent.Player!.DisplayName,
                matchEvent.TeamId!.Value,
                matchEvent.Team!.Name,
                matchEvent.Team.ClubId,
                matchEvent.Team.Club!.Name,
                matchEvent.Type
            ))
            .ToListAsync(cancellationToken);

        var rows = cardEvents
            .GroupBy(matchEvent => new
            {
                matchEvent.CompetitionId,
                matchEvent.CompetitionName,
                matchEvent.PlayerId,
                matchEvent.PlayerName,
                matchEvent.TeamId,
                matchEvent.TeamName,
                matchEvent.ClubId,
                matchEvent.ClubName
            })
            .Select(group => new DisciplineRowDraft(
                group.Key.CompetitionId,
                group.Key.CompetitionName,
                group.Key.PlayerId,
                group.Key.PlayerName,
                group.Key.TeamId,
                group.Key.TeamName,
                group.Key.ClubId,
                group.Key.ClubName,
                group.Count(matchEvent => matchEvent.Type == MatchEventType.YellowCard),
                group.Count(matchEvent => matchEvent.Type == MatchEventType.RedCard) + DoubleYellowExpulsions(group)));

        return rows
            .Select(row => new DisciplineRowDto(
                row.CompetitionId,
                row.CompetitionName,
                row.PlayerId,
                row.PlayerName,
                row.TeamId,
                row.TeamName,
                row.ClubId,
                row.ClubName,
                row.YellowCards,
                row.RedCards,
                ResolveStatus(row.YellowCards, row.RedCards)))
            .OrderBy(row => row.CompetitionName)
            .ThenByDescending(row => row.Status)
            .ThenByDescending(row => row.YellowCards)
            .ThenByDescending(row => row.RedCards)
            .ThenBy(row => row.PlayerName)
            .ToList();
    }

    public async Task<DisciplineRowDto?> GetPlayerDisciplineForMatchAsync(Guid matchId, Guid playerId, CancellationToken cancellationToken = default)
    {
        var competitionId = await dbContext.Matches
            .AsNoTracking()
            .Where(match => match.Id == matchId)
            .Select(match => (Guid?)match.CompetitionId)
            .FirstOrDefaultAsync(cancellationToken);

        if (competitionId is null)
        {
            return null;
        }

        return (await GetDisciplineAsync(null, cancellationToken))
            .FirstOrDefault(row => row.CompetitionId == competitionId && row.PlayerId == playerId);
    }

    private static DisciplineStatus ResolveStatus(int yellowCards, int redCards)
    {
        if (redCards > 0 || yellowCards >= 5)
        {
            return DisciplineStatus.Suspended;
        }

        return yellowCards == 4 ? DisciplineStatus.Warned : DisciplineStatus.Clean;
    }

    private static int DoubleYellowExpulsions(IEnumerable<CardEvent> events)
    {
        return events
            .GroupBy(matchEvent => matchEvent.MatchId)
            .Count(group => group.Count(matchEvent => matchEvent.Type == MatchEventType.YellowCard) >= 2);
    }

    private sealed record CardEvent(
        Guid MatchId,
        Guid CompetitionId,
        string CompetitionName,
        Guid PlayerId,
        string PlayerName,
        Guid TeamId,
        string TeamName,
        Guid ClubId,
        string ClubName,
        MatchEventType Type);

    private sealed record DisciplineRowDraft(
        Guid CompetitionId,
        string CompetitionName,
        Guid PlayerId,
        string PlayerName,
        Guid TeamId,
        string TeamName,
        Guid ClubId,
        string ClubName,
        int YellowCards,
        int RedCards);
}
