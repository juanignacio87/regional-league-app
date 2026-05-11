using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RegionalLeagueApp.Domain.Clubs;
using RegionalLeagueApp.Domain.Collaboration;
using RegionalLeagueApp.Domain.Competitions;
using RegionalLeagueApp.Domain.Identity;
using RegionalLeagueApp.Domain.Matches;
using RegionalLeagueApp.Domain.Players;
using RegionalLeagueApp.Domain.Standings;
using RegionalLeagueApp.Infrastructure.Identity;
using RegionalLeagueApp.Infrastructure.Persistence;
using DomainUser = RegionalLeagueApp.Domain.Identity.AppUser;

namespace RegionalLeagueApp.Infrastructure.Seed;

public sealed class DevelopmentDataSeeder(
    ApplicationDbContext db,
    UserManager<ApplicationIdentityUser> userManager,
    RoleManager<IdentityRole<Guid>> roleManager,
    IOptions<DevelopmentSeedOptions> options)
{
    private readonly DevelopmentSeedOptions seedOptions = options.Value;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (!seedOptions.Enabled)
        {
            return;
        }

        await SeedRolesAsync(cancellationToken);
        await SeedAdminAsync(cancellationToken);
        await SeedLeagueDataAsync(cancellationToken);
        await SeedCollaborationUsersAsync(cancellationToken);
    }

    private async Task SeedRolesAsync(CancellationToken cancellationToken)
    {
        foreach (var roleName in AppRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
            }
        }
    }

    private async Task SeedAdminAsync(CancellationToken cancellationToken)
    {
        var identityUser = await CreateIdentityUserAsync(
            seedOptions.AdminEmail,
            seedOptions.AdminPassword,
            seedOptions.AdminDisplayName,
            AppRoles.Admin,
            cancellationToken);
    }

    private async Task<ApplicationIdentityUser> CreateIdentityUserAsync(
        string email,
        string password,
        string displayName,
        string role,
        CancellationToken cancellationToken)
    {
        var identityUser = await userManager.FindByEmailAsync(email);
        if (identityUser is null)
        {
            identityUser = new ApplicationIdentityUser
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(identityUser, password);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Could not create seed user {email}: {string.Join(", ", result.Errors.Select(x => x.Description))}");
            }
        }

        if (!await userManager.IsInRoleAsync(identityUser, role))
        {
            await userManager.AddToRoleAsync(identityUser, role);
        }

        if (!await db.AppUsers.AnyAsync(x => x.Id == identityUser.Id, cancellationToken))
        {
            db.AppUsers.Add(new DomainUser(identityUser.Id, email, displayName));
            await db.SaveChangesAsync(cancellationToken);
        }

        return identityUser;
    }

    private async Task SeedLeagueDataAsync(CancellationToken cancellationToken)
    {
        const string leagueName = "Liga Regional Norte";

        if (await db.Leagues.AnyAsync(x => x.Name == leagueName, cancellationToken))
        {
            return;
        }

        var league = new League(leagueName, "Norte", "Argentina");
        var season = new Season(
            league.Id,
            "Temporada 2026",
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 11, 30),
            isActive: true);
        var competition = new Competition(season.Id, "Primera Division", "League");

        var rounds = new[]
        {
            new Round(competition.Id, "Fecha 1", 1),
            new Round(competition.Id, "Fecha 2", 2),
            new Round(competition.Id, "Fecha 3", 3),
            new Round(competition.Id, "Fecha 4", 4)
        };

        var venues = new[]
        {
            new Venue("Estadio Municipal", "San Pedro", 4200),
            new Venue("Cancha del Barrio", "Villa Norte", 1800),
            new Venue("Parque Central", "Rivadavia", 2600),
            new Venue("La Fortaleza", "Belgrano", 3100)
        };

        var clubSeeds = new[]
        {
            (Club: new Club("Atletico San Pedro", "ASP", "#166534", 1948, venues[0].Id), ShortName: "ASP"),
            (Club: new Club("Deportivo Norte", "DN", "#1d4ed8", 1972, venues[1].Id), ShortName: "DN"),
            (Club: new Club("Union Rivadavia", "UR", "#b91c1c", 1965, venues[2].Id), ShortName: "UR"),
            (Club: new Club("Club Belgrano", "CB", "#7c2d12", 1954, venues[3].Id), ShortName: "CB")
        };
        var clubs = clubSeeds.Select(x => x.Club).ToArray();

        var teams = clubSeeds
            .Select(seed => new Team(seed.Club.Id, competition.Id, $"{seed.ShortName} Primera", "Primera"))
            .ToArray();

        db.AddRange(league, season, competition);
        db.Rounds.AddRange(rounds);
        db.Venues.AddRange(venues);
        db.Clubs.AddRange(clubs);
        db.Teams.AddRange(teams);
        var players = CreatePlayers(teams).ToArray();
        db.Players.AddRange(players);

        var matches = new[]
        {
            new Match(competition.Id, rounds[0].Id, teams[0].Id, teams[1].Id, UtcDateTime(2026, 5, 2, 19, 0), venues[0].Id, MatchStatus.Finished, 2, 1),
            new Match(competition.Id, rounds[0].Id, teams[2].Id, teams[3].Id, UtcDateTime(2026, 5, 2, 21, 30), venues[2].Id, MatchStatus.Finished, 0, 0),
            new Match(competition.Id, rounds[1].Id, teams[1].Id, teams[2].Id, UtcDateTime(2026, 5, 9, 19, 0), venues[1].Id),
            new Match(competition.Id, rounds[1].Id, teams[3].Id, teams[0].Id, UtcDateTime(2026, 5, 9, 21, 30), venues[3].Id),
            new Match(competition.Id, rounds[2].Id, teams[0].Id, teams[2].Id, UtcDateTime(2026, 5, 16, 19, 0), venues[0].Id),
            new Match(competition.Id, rounds[2].Id, teams[1].Id, teams[3].Id, UtcDateTime(2026, 5, 16, 21, 30), venues[1].Id)
        };

        db.Matches.AddRange(matches);
        db.MatchEvents.AddRange(
            new MatchEvent(matches[0].Id, MatchEventType.KickOff, 0),
            SeedEvent(matches[0].Id, MatchEventType.Goal, 22, teams[0].Id, players, "Opening goal"),
            SeedEvent(matches[0].Id, MatchEventType.Goal, 61, teams[1].Id, players, "Equalizer"),
            SeedEvent(matches[0].Id, MatchEventType.Goal, 78, teams[0].Id, players, "Winning goal"),
            new MatchEvent(matches[0].Id, MatchEventType.FullTime, 90),
            new MatchEvent(matches[1].Id, MatchEventType.KickOff, 0),
            SeedEvent(matches[1].Id, MatchEventType.YellowCard, 34, teams[2].Id, players),
            new MatchEvent(matches[1].Id, MatchEventType.FullTime, 90));

        db.Standings.AddRange(
            new Standing(competition.Id, teams[0].Id, played: 1, won: 1, drawn: 0, lost: 0, goalsFor: 2, goalsAgainst: 1, points: 3),
            new Standing(competition.Id, teams[3].Id, played: 1, won: 0, drawn: 1, lost: 0, goalsFor: 0, goalsAgainst: 0, points: 1),
            new Standing(competition.Id, teams[2].Id, played: 1, won: 0, drawn: 1, lost: 0, goalsFor: 0, goalsAgainst: 0, points: 1),
            new Standing(competition.Id, teams[1].Id, played: 1, won: 0, drawn: 0, lost: 1, goalsFor: 1, goalsAgainst: 2, points: 0));

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedCollaborationUsersAsync(CancellationToken cancellationToken)
    {
        await CreateIdentityUserAsync(
            seedOptions.ModeratorEmail,
            seedOptions.ModeratorPassword,
            seedOptions.ModeratorDisplayName,
            AppRoles.Moderator,
            cancellationToken);

        var contributor = await CreateIdentityUserAsync(
            seedOptions.ContributorEmail,
            seedOptions.ContributorPassword,
            seedOptions.ContributorDisplayName,
            AppRoles.Contributor,
            cancellationToken);

        var assignedTeams = await db.Teams
            .AsNoTracking()
            .Include(x => x.Club)
            .OrderBy(x => x.Name)
            .Take(2)
            .Select(x => new { x.Id, x.ClubId })
            .ToListAsync(cancellationToken);

        foreach (var team in assignedTeams)
        {
            var alreadyAssigned = await db.ClubContributors.AnyAsync(
                x => x.UserId == contributor.Id && x.TeamId == team.Id,
                cancellationToken);

            if (!alreadyAssigned)
            {
                db.ClubContributors.Add(new ClubContributor(team.ClubId, team.Id, contributor.Id));
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static IEnumerable<Player> CreatePlayers(IReadOnlyList<Team> teams)
    {
        foreach (var team in teams)
        {
            var suffix = team.Name.Split(' ')[0];

            yield return new Player(team.Id, "Mateo", suffix, PlayerPosition.Goalkeeper, 1);
            yield return new Player(team.Id, "Tomas", suffix, PlayerPosition.Defender, 4);
            yield return new Player(team.Id, "Lucas", suffix, PlayerPosition.Midfielder, 8);
            yield return new Player(team.Id, "Nicolas", suffix, PlayerPosition.Forward, 9);
        }
    }

    private static MatchEvent SeedEvent(
        Guid matchId,
        MatchEventType eventType,
        int minute,
        Guid teamId,
        IReadOnlyCollection<Player> players,
        string? notes = null)
    {
        var player = players
            .Where(x => x.TeamId == teamId)
            .OrderByDescending(x => x.Position == PlayerPosition.Forward)
            .ThenBy(x => x.ShirtNumber ?? int.MaxValue)
            .First();

        return new MatchEvent(matchId, eventType, minute, teamId, player.Id, notes, player.DisplayName);
    }

    private static DateTimeOffset UtcDateTime(int year, int month, int day, int hour, int minute) =>
        new(year, month, day, hour, minute, 0, TimeSpan.Zero);
}
