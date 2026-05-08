using Microsoft.EntityFrameworkCore;
using RegionalLeagueApp.Application.Standings;
using RegionalLeagueApp.Domain.Matches;
using RegionalLeagueApp.Infrastructure.Persistence;

namespace RegionalLeagueApp.Infrastructure.Standings;

public sealed class EfStandingsQueryService(ApplicationDbContext dbContext) : IStandingsQueryService
{
    public async Task<IReadOnlyList<StandingRowDto>> GetStandingsAsync(CancellationToken cancellationToken = default)
    {
        var teams = await dbContext.Teams
            .AsNoTracking()
            .Select(team => new
            {
                team.Id,
                team.CompetitionId,
                CompetitionName = team.Competition!.Name,
                TeamId = team.Id,
                team.ClubId,
                ClubName = team.Club!.Name
            })
            .ToListAsync(cancellationToken);

        var homeRows = dbContext.Matches
            .AsNoTracking()
            .Where(match =>
                match.Status == MatchStatus.Finished &&
                match.HomeScore != null &&
                match.AwayScore != null)
            .Select(match => new
            {
                match.CompetitionId,
                TeamId = match.HomeTeamId,
                Played = 1,
                Won = match.HomeScore!.Value > match.AwayScore!.Value ? 1 : 0,
                Drawn = match.HomeScore.Value == match.AwayScore.Value ? 1 : 0,
                Lost = match.HomeScore.Value < match.AwayScore.Value ? 1 : 0,
                GoalsFor = match.HomeScore.Value,
                GoalsAgainst = match.AwayScore.Value,
                Points = match.HomeScore.Value > match.AwayScore.Value ? 3 : match.HomeScore.Value == match.AwayScore.Value ? 1 : 0
            });

        var awayRows = dbContext.Matches
            .AsNoTracking()
            .Where(match =>
                match.Status == MatchStatus.Finished &&
                match.HomeScore != null &&
                match.AwayScore != null)
            .Select(match => new
            {
                match.CompetitionId,
                TeamId = match.AwayTeamId,
                Played = 1,
                Won = match.AwayScore!.Value > match.HomeScore!.Value ? 1 : 0,
                Drawn = match.AwayScore.Value == match.HomeScore.Value ? 1 : 0,
                Lost = match.AwayScore.Value < match.HomeScore.Value ? 1 : 0,
                GoalsFor = match.AwayScore.Value,
                GoalsAgainst = match.HomeScore.Value,
                Points = match.AwayScore.Value > match.HomeScore.Value ? 3 : match.AwayScore.Value == match.HomeScore.Value ? 1 : 0
            });

        var stats = await homeRows
            .Concat(awayRows)
            .GroupBy(row => new { row.CompetitionId, row.TeamId })
            .Select(group => new
            {
                group.Key.CompetitionId,
                group.Key.TeamId,
                Played = group.Sum(row => row.Played),
                Won = group.Sum(row => row.Won),
                Drawn = group.Sum(row => row.Drawn),
                Lost = group.Sum(row => row.Lost),
                GoalsFor = group.Sum(row => row.GoalsFor),
                GoalsAgainst = group.Sum(row => row.GoalsAgainst),
                Points = group.Sum(row => row.Points)
            })
            .ToListAsync(cancellationToken);

        var statsByTeam = stats.ToDictionary(row => (row.CompetitionId, row.TeamId));

        return teams
            .Select(team =>
            {
                statsByTeam.TryGetValue((team.CompetitionId, team.Id), out var stat);

                var goalsFor = stat?.GoalsFor ?? 0;
                var goalsAgainst = stat?.GoalsAgainst ?? 0;

                return new StandingRowDraft(
                    team.CompetitionId,
                    team.CompetitionName,
                    team.TeamId,
                    team.ClubId,
                    team.ClubName,
                    stat?.Played ?? 0,
                    stat?.Won ?? 0,
                    stat?.Drawn ?? 0,
                    stat?.Lost ?? 0,
                    goalsFor,
                    goalsAgainst,
                    goalsFor - goalsAgainst,
                    stat?.Points ?? 0);
            })
            .OrderByDescending(row => row.Points)
            .ThenByDescending(row => row.GoalDifference)
            .ThenByDescending(row => row.GoalsFor)
            .ThenBy(row => row.ClubName)
            .Select((row, index) => new StandingRowDto(
                index + 1,
                row.CompetitionId,
                row.CompetitionName,
                row.TeamId,
                row.ClubId,
                row.ClubName,
                row.Played,
                row.Won,
                row.Drawn,
                row.Lost,
                row.GoalsFor,
                row.GoalsAgainst,
                row.GoalDifference,
                row.Points))
            .ToList();
    }

    private sealed record StandingRowDraft(
        Guid CompetitionId,
        string CompetitionName,
        Guid TeamId,
        Guid ClubId,
        string ClubName,
        int Played,
        int Won,
        int Drawn,
        int Lost,
        int GoalsFor,
        int GoalsAgainst,
        int GoalDifference,
        int Points);
}
