using Microsoft.EntityFrameworkCore;
using RegionalLeagueApp.Application.Scorers;
using RegionalLeagueApp.Domain.Matches;
using RegionalLeagueApp.Infrastructure.Persistence;

namespace RegionalLeagueApp.Infrastructure.Scorers;

public sealed class EfScorersQueryService(ApplicationDbContext dbContext) : IScorersQueryService
{
    public async Task<IReadOnlyList<ScorerRowDto>> GetScorersAsync(Guid? leagueId = null, Guid? competitionId = null, CancellationToken cancellationToken = default)
    {
        var goalEvents = await dbContext.MatchEvents
            .AsNoTracking()
            .Where(matchEvent =>
                matchEvent.Type == MatchEventType.Goal &&
                (matchEvent.PlayerId != null || matchEvent.PlayerName != string.Empty) &&
                matchEvent.TeamId != null &&
                matchEvent.Team!.IsActive &&
                matchEvent.Team.Club!.IsActive &&
                matchEvent.Match!.Competition!.IsActive &&
                matchEvent.Match.Competition.Season!.League!.IsActive &&
                (leagueId == null || matchEvent.Match.Competition.Season.LeagueId == leagueId) &&
                (competitionId == null || matchEvent.Match.CompetitionId == competitionId) &&
                (matchEvent.Player == null || matchEvent.Player.IsActive))
            .Select(matchEvent => new GoalEvent(
                matchEvent.PlayerId,
                matchEvent.PlayerName != string.Empty
                    ? matchEvent.PlayerName
                    : matchEvent.Player != null
                        ? matchEvent.Player.DisplayName
                        : string.Empty,
                matchEvent.TeamId!.Value,
                matchEvent.Team!.Club!.ShortName != string.Empty ? matchEvent.Team.Club.ShortName : matchEvent.Team.Club.Name,
                matchEvent.Match!.CompetitionId,
                matchEvent.Match.Competition!.Name))
            .ToListAsync(cancellationToken);

        return goalEvents
            .Where(matchEvent => !string.IsNullOrWhiteSpace(matchEvent.PlayerName))
            .GroupBy(matchEvent => new
            {
                matchEvent.CompetitionId,
                matchEvent.CompetitionName,
                matchEvent.TeamId,
                matchEvent.TeamName,
                PlayerName = Normalize(matchEvent.PlayerName)
            })
            .Select(group =>
            {
                var canonical = group
                    .OrderByDescending(matchEvent => matchEvent.PlayerId is not null)
                    .ThenBy(matchEvent => matchEvent.PlayerName)
                    .First();

                return new ScorerRowDto(
                    canonical.PlayerId,
                    canonical.PlayerName.Trim(),
                    group.Key.TeamId,
                    group.Key.TeamName,
                    group.Key.CompetitionId,
                    group.Key.CompetitionName,
                    group.Count());
            })
            .OrderByDescending(row => row.Goals)
            .ThenBy(row => row.PlayerName)
            .ToList();
    }

    private static string Normalize(string value) =>
        string.Join(' ', value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)).ToUpperInvariant();

    private sealed record GoalEvent(
        Guid? PlayerId,
        string PlayerName,
        Guid TeamId,
        string TeamName,
        Guid CompetitionId,
        string CompetitionName);
}
