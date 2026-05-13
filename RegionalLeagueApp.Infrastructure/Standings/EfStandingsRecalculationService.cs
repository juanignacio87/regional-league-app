using Microsoft.EntityFrameworkCore;
using RegionalLeagueApp.Application.Standings;
using RegionalLeagueApp.Domain.Standings;
using RegionalLeagueApp.Infrastructure.Persistence;

namespace RegionalLeagueApp.Infrastructure.Standings;

public sealed class EfStandingsRecalculationService(
    ApplicationDbContext dbContext,
    IStandingsQueryService standingsQueryService) : IStandingsRecalculationService
{
    public async Task RecalculateCompetitionAsync(Guid competitionId, CancellationToken cancellationToken = default)
    {
        await dbContext.Standings
            .Where(row => row.CompetitionId == competitionId)
            .ExecuteDeleteAsync(cancellationToken);

        var rows = await standingsQueryService.GetStandingsAsync(cancellationToken: cancellationToken);
        var competitionRows = rows
            .Where(row => row.CompetitionId == competitionId)
            .Select(row => new Standing(
                row.CompetitionId,
                row.TeamId,
                row.Played,
                row.Won,
                row.Drawn,
                row.Lost,
                row.GoalsFor,
                row.GoalsAgainst,
                row.Points));

        dbContext.Standings.AddRange(competitionRows);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
