using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RegionalLeagueApp.Application.Discipline;
using RegionalLeagueApp.Application.Players;
using RegionalLeagueApp.Application.Scorers;
using RegionalLeagueApp.Domain.Clubs;
using RegionalLeagueApp.Domain.Competitions;
using RegionalLeagueApp.Domain.Matches;
using RegionalLeagueApp.Domain.Players;
using RegionalLeagueApp.Infrastructure.Discipline;
using RegionalLeagueApp.Infrastructure.Persistence;
using RegionalLeagueApp.Infrastructure.Players;
using RegionalLeagueApp.Infrastructure.Scorers;
using Xunit;

namespace RegionalLeagueApp.Tests;

public sealed class DataIntegrityTests
{
    [Fact]
    public async Task GetScorersAsync_merges_player_id_and_snapshot_name_goals()
    {
        await using var scope = await DataIntegrityScope.CreateAsync();
        var player = await scope.AddPlayerAsync(scope.HomeTeamId, "Juan", "Perez", 9);
        scope.Db.MatchEvents.Add(new MatchEvent(scope.MatchId, MatchEventType.Goal, 10, scope.HomeTeamId, player.Id, playerName: "Juan Perez"));
        scope.Db.MatchEvents.Add(new MatchEvent(scope.MatchId, MatchEventType.Goal, 20, scope.HomeTeamId, playerName: " juan   perez "));
        await scope.Db.SaveChangesAsync();

        var rows = await scope.Scorers.GetScorersAsync(scope.LeagueId, scope.CompetitionId);

        var row = Assert.Single(rows);
        Assert.Equal(player.Id, row.PlayerId);
        Assert.Equal(2, row.Goals);
    }

    [Fact]
    public async Task GetScorersAsync_groups_snapshot_name_goals_without_player_id()
    {
        await using var scope = await DataIntegrityScope.CreateAsync();
        scope.Db.MatchEvents.Add(new MatchEvent(scope.MatchId, MatchEventType.Goal, 10, scope.HomeTeamId, playerName: "Pedro Gomez"));
        scope.Db.MatchEvents.Add(new MatchEvent(scope.MatchId, MatchEventType.Goal, 20, scope.HomeTeamId, playerName: " pedro   gomez "));
        await scope.Db.SaveChangesAsync();

        var rows = await scope.Scorers.GetScorersAsync(scope.LeagueId, scope.CompetitionId);

        var row = Assert.Single(rows);
        Assert.Null(row.PlayerId);
        Assert.Equal(2, row.Goals);
    }

    [Fact]
    public async Task GetDisciplineAsync_includes_card_with_snapshot_name_without_player_id()
    {
        await using var scope = await DataIntegrityScope.CreateAsync();
        scope.Db.MatchEvents.Add(new MatchEvent(scope.MatchId, MatchEventType.YellowCard, 35, scope.HomeTeamId, playerName: "Pedro Gomez"));
        await scope.Db.SaveChangesAsync();

        var rows = await scope.Discipline.GetDisciplineAsync(scope.LeagueId);

        var row = Assert.Single(rows);
        Assert.Null(row.PlayerId);
        Assert.Equal("Pedro Gomez", row.PlayerName);
        Assert.Equal(1, row.YellowCards);
        Assert.Equal(0, row.RedCards);
        Assert.Equal(DisciplineStatus.Clean, row.Status);
    }

    [Fact]
    public async Task ManualPlayerValidation_rejects_duplicate_display_name_in_same_team()
    {
        await using var scope = await DataIntegrityScope.CreateAsync();
        await scope.AddPlayerAsync(scope.HomeTeamId, "Juan", "Perez", 9);

        var message = await scope.ManualPlayerValidation.ValidateAsync(new ManualPlayerValidationRequest(
            scope.HomeTeamId,
            " juan   perez ",
            10,
            true,
            null));

        Assert.NotNull(message);
        Assert.Contains("display name", message, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class DataIntegrityScope : IAsyncDisposable
    {
        private readonly SqliteConnection connection;

        private DataIntegrityScope(SqliteConnection connection, ApplicationDbContext db)
        {
            this.connection = connection;
            Db = db;
            Scorers = new EfScorersQueryService(db);
            Discipline = new EfDisciplineQueryService(db);
            ManualPlayerValidation = new EfManualPlayerValidationService(db);
        }

        public ApplicationDbContext Db { get; }
        public IScorersQueryService Scorers { get; }
        public IDisciplineQueryService Discipline { get; }
        public IManualPlayerValidationService ManualPlayerValidation { get; }
        public Guid LeagueId { get; private init; }
        public Guid CompetitionId { get; private init; }
        public Guid HomeTeamId { get; private init; }
        public Guid AwayTeamId { get; private init; }
        public Guid MatchId { get; private init; }

        public static async Task<DataIntegrityScope> CreateAsync()
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
            var round = new Round(competition.Id, "Fecha 1", 1);
            var homeClub = new Club("Club A", "A");
            var awayClub = new Club("Club B", "B");
            var homeTeam = new Team(homeClub.Id, competition.Id, "A Primera", "Primera");
            var awayTeam = new Team(awayClub.Id, competition.Id, "B Primera", "Primera");
            var match = new Match(competition.Id, round.Id, homeTeam.Id, awayTeam.Id, new DateTimeOffset(2026, 8, 1, 14, 0, 0, TimeSpan.Zero));

            db.Leagues.Add(league);
            db.Seasons.Add(season);
            db.Competitions.Add(competition);
            db.Rounds.Add(round);
            db.Clubs.AddRange(homeClub, awayClub);
            db.Teams.AddRange(homeTeam, awayTeam);
            db.Matches.Add(match);
            await db.SaveChangesAsync();

            return new DataIntegrityScope(connection, db)
            {
                LeagueId = league.Id,
                CompetitionId = competition.Id,
                HomeTeamId = homeTeam.Id,
                AwayTeamId = awayTeam.Id,
                MatchId = match.Id
            };
        }

        public async Task<Player> AddPlayerAsync(Guid teamId, string firstName, string lastName, int shirtNumber)
        {
            var player = new Player(teamId, firstName, lastName, PlayerPosition.Unknown, shirtNumber);
            Db.Players.Add(player);
            await Db.SaveChangesAsync();
            return player;
        }

        public async ValueTask DisposeAsync()
        {
            await Db.DisposeAsync();
            await connection.DisposeAsync();
        }
    }
}
