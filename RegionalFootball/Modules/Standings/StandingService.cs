using Microsoft.EntityFrameworkCore;
using RegionalFootball.Data;
using RegionalFootball.Modules.Matches;

namespace RegionalFootball.Modules.Standings;

public class StandingService(ApplicationDbContext db)
{
    public async Task RebuildCompetitionAsync(int competitionId, CancellationToken cancellationToken = default)
    {
        var existing = await db.Standings
            .Where(x => x.CompetitionId == competitionId)
            .ToListAsync(cancellationToken);
        db.Standings.RemoveRange(existing);

        var matches = await db.Matches
            .Include(x => x.Round)
            .Where(x => x.Round != null && x.Round.CompetitionId == competitionId && x.Status == MatchStatus.Finished)
            .ToListAsync(cancellationToken);

        var table = new Dictionary<int, Standing>();
        foreach (var match in matches)
        {
            if (match.HomeScore is null || match.AwayScore is null)
            {
                continue;
            }

            var home = GetOrCreate(table, competitionId, match.HomeTeamId);
            var away = GetOrCreate(table, competitionId, match.AwayTeamId);

            home.Played++;
            away.Played++;
            home.GoalsFor += match.HomeScore.Value;
            home.GoalsAgainst += match.AwayScore.Value;
            away.GoalsFor += match.AwayScore.Value;
            away.GoalsAgainst += match.HomeScore.Value;

            if (match.HomeScore > match.AwayScore)
            {
                home.Won++;
                away.Lost++;
                home.Points += 3;
            }
            else if (match.HomeScore < match.AwayScore)
            {
                away.Won++;
                home.Lost++;
                away.Points += 3;
            }
            else
            {
                home.Drawn++;
                away.Drawn++;
                home.Points++;
                away.Points++;
            }
        }

        db.Standings.AddRange(table.Values);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static Standing GetOrCreate(Dictionary<int, Standing> table, int competitionId, int teamId)
    {
        if (!table.TryGetValue(teamId, out var standing))
        {
            standing = new Standing { CompetitionId = competitionId, TeamId = teamId };
            table[teamId] = standing;
        }

        return standing;
    }
}
