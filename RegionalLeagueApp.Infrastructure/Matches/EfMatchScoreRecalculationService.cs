using Microsoft.EntityFrameworkCore;
using RegionalLeagueApp.Application.Matches;
using RegionalLeagueApp.Application.Standings;
using RegionalLeagueApp.Domain.Matches;
using RegionalLeagueApp.Infrastructure.Persistence;

namespace RegionalLeagueApp.Infrastructure.Matches;

public sealed class EfMatchScoreRecalculationService(
    ApplicationDbContext dbContext,
    IStandingsRecalculationService standingsRecalculationService) : IMatchScoreRecalculationService
{
    public async Task RecalculateMatchScoreAsync(Guid matchId, CancellationToken cancellationToken = default)
    {
        await RecalculateMatchScoreCoreAsync(matchId, cancellationToken);
    }

    public async Task RecalculateMatchScoreAndStandingsAsync(Guid matchId, CancellationToken cancellationToken = default)
    {
        var competitionId = await RecalculateMatchScoreCoreAsync(matchId, cancellationToken);
        await standingsRecalculationService.RecalculateCompetitionAsync(competitionId, cancellationToken);
    }

    private async Task<Guid> RecalculateMatchScoreCoreAsync(Guid matchId, CancellationToken cancellationToken)
    {
        var match = await dbContext.Matches.FirstOrDefaultAsync(x => x.Id == matchId, cancellationToken);
        if (match is null)
        {
            throw new InvalidOperationException("Match not found.");
        }

        var goalsByTeam = await dbContext.MatchEvents
            .AsNoTracking()
            .Where(x => x.MatchId == matchId && x.Type == MatchEventType.Goal && x.TeamId != null)
            .GroupBy(x => x.TeamId!.Value)
            .Select(group => new { TeamId = group.Key, Goals = group.Count() })
            .ToListAsync(cancellationToken);

        var homeScore = goalsByTeam.FirstOrDefault(x => x.TeamId == match.HomeTeamId)?.Goals ?? 0;
        var awayScore = goalsByTeam.FirstOrDefault(x => x.TeamId == match.AwayTeamId)?.Goals ?? 0;

        dbContext.Entry(match).Property(x => x.HomeScore).CurrentValue = homeScore;
        dbContext.Entry(match).Property(x => x.AwayScore).CurrentValue = awayScore;
        await dbContext.SaveChangesAsync(cancellationToken);

        return match.CompetitionId;
    }
}
