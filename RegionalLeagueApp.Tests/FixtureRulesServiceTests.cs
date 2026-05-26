using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RegionalLeagueApp.Application.Fixtures;
using RegionalLeagueApp.Domain.Clubs;
using RegionalLeagueApp.Domain.Competitions;
using RegionalLeagueApp.Domain.Matches;
using RegionalLeagueApp.Infrastructure.Fixtures;
using RegionalLeagueApp.Infrastructure.Persistence;
using Xunit;

namespace RegionalLeagueApp.Tests;

public sealed class FixtureRulesServiceTests
{
    [Fact]
    public async Task ValidateMatchCreationAsync_rejects_same_home_and_away_team()
    {
        await using var scope = await FixtureTestScope.CreateAsync();

        var result = await scope.Service.ValidateMatchCreationAsync(scope.Request(
            "Fecha 1",
            1,
            scope.HomeTeamId,
            scope.HomeTeamId,
            FixtureTestScope.Utc(2026, 8, 1, 14, 0)));

        Assert.False(result.IsValid);
        Assert.Contains("distintos", result.FirstError);
    }

    [Fact]
    public async Task ValidateMatchCreationAsync_rejects_team_playing_twice_in_same_round()
    {
        await using var scope = await FixtureTestScope.CreateAsync();
        await scope.AddMatchAsync("Fecha 1", 1, scope.HomeTeamId, scope.AwayTeamId, FixtureTestScope.Utc(2026, 8, 1, 14, 0));

        var result = await scope.Service.ValidateMatchCreationAsync(scope.Request(
            "Fecha 1",
            1,
            scope.HomeTeamId,
            scope.ThirdTeamId,
            FixtureTestScope.Utc(2026, 8, 1, 16, 0)));

        Assert.False(result.IsValid);
        Assert.Contains("misma jornada", result.FirstError);
    }

    [Fact]
    public async Task ValidateMatchUpdateAsync_excludes_current_match_from_validation()
    {
        await using var scope = await FixtureTestScope.CreateAsync();
        var match = await scope.AddMatchAsync("Fecha 1", 1, scope.HomeTeamId, scope.AwayTeamId, FixtureTestScope.Utc(2026, 8, 1, 14, 0));

        var result = await scope.Service.ValidateMatchUpdateAsync(match.Id, scope.Request(
            "Fecha 1",
            1,
            scope.HomeTeamId,
            scope.AwayTeamId,
            match.StartsAt));

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidateMatchCreationAsync_rejects_exact_duplicate()
    {
        await using var scope = await FixtureTestScope.CreateAsync();
        var startsAt = FixtureTestScope.Utc(2026, 8, 1, 14, 0);
        await scope.AddMatchAsync("Fecha 1", 1, scope.HomeTeamId, scope.AwayTeamId, startsAt);

        var result = await scope.Service.ValidateMatchCreationAsync(scope.Request(
            "Fecha 2",
            2,
            scope.HomeTeamId,
            scope.AwayTeamId,
            startsAt));

        Assert.False(result.IsValid);
        Assert.Contains("mismo local, visitante y fecha/hora", result.FirstError);
    }

    [Fact]
    public async Task ValidateMatchCreationAsync_allows_same_teams_in_different_rounds()
    {
        await using var scope = await FixtureTestScope.CreateAsync();
        await scope.AddMatchAsync("Fecha 1", 1, scope.HomeTeamId, scope.AwayTeamId, FixtureTestScope.Utc(2026, 8, 1, 14, 0));

        var result = await scope.Service.ValidateMatchCreationAsync(scope.Request(
            "Fecha 2",
            2,
            scope.HomeTeamId,
            scope.AwayTeamId,
            FixtureTestScope.Utc(2026, 8, 8, 14, 0)));

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task HasBlockingMatchesForRegenerationAsync_returns_true_when_match_has_events()
    {
        await using var scope = await FixtureTestScope.CreateAsync();
        var match = await scope.AddMatchAsync("Fecha 1", 1, scope.HomeTeamId, scope.AwayTeamId, FixtureTestScope.Utc(2026, 8, 1, 14, 0));
        scope.Db.MatchEvents.Add(new MatchEvent(match.Id, MatchEventType.Goal, 10, scope.HomeTeamId, playerName: "Jugador"));
        await scope.Db.SaveChangesAsync();

        var result = await scope.Service.HasBlockingMatchesForRegenerationAsync(scope.CompetitionId);

        Assert.True(result);
    }

    [Fact]
    public async Task GetSafeDeletableMatchIdsAsync_returns_scheduled_match_without_events_or_score()
    {
        await using var scope = await FixtureTestScope.CreateAsync();
        var match = await scope.AddMatchAsync("Fecha 1", 1, scope.HomeTeamId, scope.AwayTeamId, FixtureTestScope.Utc(2026, 8, 1, 14, 0));

        var ids = await scope.Service.GetSafeDeletableMatchIdsAsync(scope.CompetitionId);

        Assert.Contains(match.Id, ids);
        Assert.False(await scope.Service.HasBlockingMatchesForRegenerationAsync(scope.CompetitionId));
    }

    [Fact]
    public async Task ValidateFixtureBatchAsync_detects_conflicts_inside_batch()
    {
        await using var scope = await FixtureTestScope.CreateAsync();

        var result = await scope.Service.ValidateFixtureBatchAsync(scope.CompetitionId,
        [
            scope.Batch("Fecha 1 - A vs B", "Fecha 1", 1, scope.HomeTeamId, scope.AwayTeamId, FixtureTestScope.Utc(2026, 8, 1, 14, 0)),
            scope.Batch("Fecha 1 - A vs C", "Fecha 1", 1, scope.HomeTeamId, scope.ThirdTeamId, FixtureTestScope.Utc(2026, 8, 1, 16, 0))
        ]);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("dentro del lote", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ValidateFixtureBatchAsync_detects_conflict_against_existing_matches()
    {
        await using var scope = await FixtureTestScope.CreateAsync();
        await scope.AddMatchAsync("Fecha 1", 1, scope.HomeTeamId, scope.AwayTeamId, FixtureTestScope.Utc(2026, 8, 1, 14, 0));

        var result = await scope.Service.ValidateFixtureBatchAsync(scope.CompetitionId,
        [
            scope.Batch("Fecha 1 - A vs C", "Fecha 1", 1, scope.HomeTeamId, scope.ThirdTeamId, FixtureTestScope.Utc(2026, 8, 1, 16, 0))
        ]);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("misma jornada", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ValidateMatchCreationAsync_rejects_inactive_team_or_archived_club()
    {
        await using var scope = await FixtureTestScope.CreateAsync();

        var inactiveClub = new Club("Inactivo", "INA");
        var inactiveTeam = new Team(inactiveClub.Id, scope.CompetitionId, "Inactivo", "Primera");
        inactiveTeam.SetActive(false);
        var archivedClub = new Club("Archivado", "ARC");
        archivedClub.SetActive(false);
        var archivedClubTeam = new Team(archivedClub.Id, scope.CompetitionId, "Archivado Primera", "Primera");
        scope.Db.Clubs.AddRange(inactiveClub, archivedClub);
        scope.Db.Teams.AddRange(inactiveTeam, archivedClubTeam);
        await scope.Db.SaveChangesAsync();

        var inactiveTeamResult = await scope.Service.ValidateMatchCreationAsync(scope.Request(
            "Fecha 1",
            1,
            inactiveTeam.Id,
            scope.AwayTeamId,
            FixtureTestScope.Utc(2026, 8, 1, 14, 0)));
        var archivedClubResult = await scope.Service.ValidateMatchCreationAsync(scope.Request(
            "Fecha 2",
            2,
            archivedClubTeam.Id,
            scope.AwayTeamId,
            FixtureTestScope.Utc(2026, 8, 8, 14, 0)));

        Assert.False(inactiveTeamResult.IsValid);
        Assert.False(archivedClubResult.IsValid);
        Assert.Contains("activos", inactiveTeamResult.FirstError);
        Assert.Contains("activos", archivedClubResult.FirstError);
    }

    private sealed class FixtureTestScope : IAsyncDisposable
    {
        private readonly SqliteConnection connection;

        private FixtureTestScope(SqliteConnection connection, ApplicationDbContext db)
        {
            this.connection = connection;
            Db = db;
            Service = new EfFixtureRulesService(db);
        }

        public ApplicationDbContext Db { get; }
        public IFixtureRulesService Service { get; }
        public Guid CompetitionId { get; private init; }
        public Guid ActiveClubId { get; private init; }
        public Guid HomeTeamId { get; private init; }
        public Guid AwayTeamId { get; private init; }
        public Guid ThirdTeamId { get; private init; }

        public static async Task<FixtureTestScope> CreateAsync()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;
            var db = new ApplicationDbContext(options);
            await db.Database.EnsureCreatedAsync();

            var league = new League("Liga Test", "Region", "AR");
            var season = new Season(league.Id, "2026", new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), true);
            var competition = new Competition(season.Id, "Primera");

            var homeClub = new Club("Club A", "A");
            var awayClub = new Club("Club B", "B");
            var thirdClub = new Club("Club C", "C");
            var homeTeam = new Team(homeClub.Id, competition.Id, "A Primera", "Primera");
            var awayTeam = new Team(awayClub.Id, competition.Id, "B Primera", "Primera");
            var thirdTeam = new Team(thirdClub.Id, competition.Id, "C Primera", "Primera");

            db.Leagues.Add(league);
            db.Seasons.Add(season);
            db.Competitions.Add(competition);
            db.Clubs.AddRange(homeClub, awayClub, thirdClub);
            db.Teams.AddRange(homeTeam, awayTeam, thirdTeam);
            await db.SaveChangesAsync();

            return new FixtureTestScope(connection, db)
            {
                CompetitionId = competition.Id,
                ActiveClubId = homeClub.Id,
                HomeTeamId = homeTeam.Id,
                AwayTeamId = awayTeam.Id,
                ThirdTeamId = thirdTeam.Id
            };
        }

        public FixtureMatchRulesRequest Request(
            string roundName,
            int roundSortOrder,
            Guid homeTeamId,
            Guid awayTeamId,
            DateTimeOffset startsAt) =>
            new(CompetitionId, roundName, roundSortOrder, homeTeamId, awayTeamId, startsAt);

        public FixtureBatchMatchRequest Batch(
            string label,
            string roundName,
            int roundSortOrder,
            Guid homeTeamId,
            Guid awayTeamId,
            DateTimeOffset startsAt) =>
            new(label, roundName, roundSortOrder, homeTeamId, awayTeamId, startsAt);

        public async Task<Match> AddMatchAsync(
            string roundName,
            int roundSortOrder,
            Guid homeTeamId,
            Guid awayTeamId,
            DateTimeOffset startsAt,
            MatchStatus status = MatchStatus.Scheduled)
        {
            var round = await Db.Rounds.FirstOrDefaultAsync(x => x.CompetitionId == CompetitionId && x.SortOrder == roundSortOrder);
            if (round is null)
            {
                round = new Round(CompetitionId, roundName, roundSortOrder);
                Db.Rounds.Add(round);
                await Db.SaveChangesAsync();
            }

            var match = new Match(CompetitionId, round.Id, homeTeamId, awayTeamId, startsAt, status: status);
            Db.Matches.Add(match);
            await Db.SaveChangesAsync();
            return match;
        }

        public static DateTimeOffset Utc(int year, int month, int day, int hour, int minute) =>
            new(year, month, day, hour, minute, 0, TimeSpan.Zero);

        public async ValueTask DisposeAsync()
        {
            await Db.DisposeAsync();
            await connection.DisposeAsync();
        }
    }
}
