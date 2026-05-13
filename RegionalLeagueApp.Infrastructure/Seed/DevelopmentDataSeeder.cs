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
        const string leagueRegion = "Norte";
        const string leagueCountry = "Argentina";
        const string seasonName = "Temporada 2026";
        const string competitionName = "Primera Division";

        var league = await db.Leagues.FirstOrDefaultAsync(
            x => x.Name == leagueName && x.Region == leagueRegion,
            cancellationToken);
        if (league is null)
        {
            league = new League(leagueName, leagueRegion, leagueCountry);
            db.Leagues.Add(league);
        }

        var season = await db.Seasons.FirstOrDefaultAsync(
            x => x.LeagueId == league.Id && x.Name == seasonName,
            cancellationToken);
        if (season is null)
        {
            season = new Season(
                league.Id,
                seasonName,
                new DateOnly(2026, 3, 1),
                new DateOnly(2026, 11, 30),
                isActive: true);
            db.Seasons.Add(season);
        }

        var competition = await db.Competitions.FirstOrDefaultAsync(
            x => x.SeasonId == season.Id && x.Name == competitionName,
            cancellationToken);
        if (competition is null)
        {
            competition = new Competition(season.Id, competitionName, "League");
            db.Competitions.Add(competition);
        }

        var rounds = await EnsureRoundsAsync(competition.Id, cancellationToken);
        var venues = await EnsureVenuesAsync(cancellationToken);
        var clubs = await EnsureClubsAsync(venues, cancellationToken);
        var teams = await EnsureTeamsAsync(clubs, competition.Id, cancellationToken);
        var players = await EnsurePlayersAsync(teams, cancellationToken);
        var matches = await EnsureMatchesAsync(competition.Id, rounds, teams, venues, cancellationToken);

        await EnsureMatchEventsAsync(matches, teams, players, cancellationToken);
        await EnsureStandingsAsync(competition.Id, teams, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<Round[]> EnsureRoundsAsync(Guid competitionId, CancellationToken cancellationToken)
    {
        var seedRounds = new[]
        {
            (Name: "Fecha 1", SortOrder: 1),
            (Name: "Fecha 2", SortOrder: 2),
            (Name: "Fecha 3", SortOrder: 3),
            (Name: "Fecha 4", SortOrder: 4)
        };

        var rounds = new List<Round>();
        foreach (var seed in seedRounds)
        {
            var round = await db.Rounds.FirstOrDefaultAsync(
                x => x.CompetitionId == competitionId && x.SortOrder == seed.SortOrder,
                cancellationToken);

            if (round is null)
            {
                round = new Round(competitionId, seed.Name, seed.SortOrder);
                db.Rounds.Add(round);
            }

            rounds.Add(round);
        }

        return rounds.ToArray();
    }

    private async Task<Venue[]> EnsureVenuesAsync(CancellationToken cancellationToken)
    {
        var seedVenues = new[]
        {
            (Name: "Estadio Municipal", City: "San Pedro", Capacity: 4200),
            (Name: "Cancha del Barrio", City: "Villa Norte", Capacity: 1800),
            (Name: "Parque Central", City: "Rivadavia", Capacity: 2600),
            (Name: "La Fortaleza", City: "Belgrano", Capacity: 3100)
        };

        var venues = new List<Venue>();
        foreach (var seed in seedVenues)
        {
            var venue = await db.Venues.FirstOrDefaultAsync(
                x => x.Name == seed.Name && x.City == seed.City,
                cancellationToken);

            if (venue is null)
            {
                venue = new Venue(seed.Name, seed.City, seed.Capacity);
                db.Venues.Add(venue);
            }

            venues.Add(venue);
        }

        return venues.ToArray();
    }

    private async Task<Club[]> EnsureClubsAsync(IReadOnlyList<Venue> venues, CancellationToken cancellationToken)
    {
        var seedClubs = new[]
        {
            (Name: "Atletico San Pedro", ShortName: "ASP", PrimaryColor: "#166534", FoundedYear: 1948, Venue: venues[0]),
            (Name: "Deportivo Norte", ShortName: "DN", PrimaryColor: "#1d4ed8", FoundedYear: 1972, Venue: venues[1]),
            (Name: "Union Rivadavia", ShortName: "UR", PrimaryColor: "#b91c1c", FoundedYear: 1965, Venue: venues[2]),
            (Name: "Club Belgrano", ShortName: "CB", PrimaryColor: "#7c2d12", FoundedYear: 1954, Venue: venues[3])
        };

        var clubs = new List<Club>();
        foreach (var seed in seedClubs)
        {
            var club = await db.Clubs.FirstOrDefaultAsync(x => x.Name == seed.Name, cancellationToken);
            if (club is null)
            {
                club = new Club(seed.Name, seed.ShortName, seed.PrimaryColor, seed.FoundedYear, seed.Venue.Id);
                db.Clubs.Add(club);
            }

            clubs.Add(club);
        }

        return clubs.ToArray();
    }

    private async Task<Team[]> EnsureTeamsAsync(IReadOnlyList<Club> clubs, Guid competitionId, CancellationToken cancellationToken)
    {
        var teams = new List<Team>();
        foreach (var club in clubs)
        {
            var teamName = $"{club.ShortName} Primera";
            var team = await db.Teams.FirstOrDefaultAsync(
                x => x.CompetitionId == competitionId && x.ClubId == club.Id,
                cancellationToken);

            if (team is null)
            {
                team = new Team(club.Id, competitionId, teamName, "Primera");
                db.Teams.Add(team);
            }

            teams.Add(team);
        }

        return teams.ToArray();
    }

    private async Task<Player[]> EnsurePlayersAsync(IReadOnlyList<Team> teams, CancellationToken cancellationToken)
    {
        var players = new List<Player>();
        foreach (var team in teams)
        {
            var existingPlayers = await db.Players
                .Where(x => x.TeamId == team.Id)
                .ToListAsync(cancellationToken);

            foreach (var seedPlayer in CreatePlayers([team]))
            {
                var player = existingPlayers.FirstOrDefault(x => x.ShirtNumber == seedPlayer.ShirtNumber);
                if (player is null)
                {
                    player = seedPlayer;
                    db.Players.Add(player);
                    existingPlayers.Add(player);
                }

                players.Add(player);
            }
        }

        return players.ToArray();
    }

    private async Task<Match[]> EnsureMatchesAsync(
        Guid competitionId,
        IReadOnlyList<Round> rounds,
        IReadOnlyList<Team> teams,
        IReadOnlyList<Venue> venues,
        CancellationToken cancellationToken)
    {
        var seedMatches = new[]
        {
            (Round: rounds[0], Home: teams[0], Away: teams[1], StartsAt: UtcDateTime(2026, 5, 2, 19, 0), Venue: venues[0], Status: MatchStatus.Finished, HomeScore: (int?)2, AwayScore: (int?)1),
            (Round: rounds[0], Home: teams[2], Away: teams[3], StartsAt: UtcDateTime(2026, 5, 2, 21, 30), Venue: venues[2], Status: MatchStatus.Finished, HomeScore: (int?)0, AwayScore: (int?)0),
            (Round: rounds[1], Home: teams[1], Away: teams[2], StartsAt: UtcDateTime(2026, 5, 9, 19, 0), Venue: venues[1], Status: MatchStatus.Scheduled, HomeScore: (int?)null, AwayScore: (int?)null),
            (Round: rounds[1], Home: teams[3], Away: teams[0], StartsAt: UtcDateTime(2026, 5, 9, 21, 30), Venue: venues[3], Status: MatchStatus.Scheduled, HomeScore: (int?)null, AwayScore: (int?)null),
            (Round: rounds[2], Home: teams[0], Away: teams[2], StartsAt: UtcDateTime(2026, 5, 16, 19, 0), Venue: venues[0], Status: MatchStatus.Scheduled, HomeScore: (int?)null, AwayScore: (int?)null),
            (Round: rounds[2], Home: teams[1], Away: teams[3], StartsAt: UtcDateTime(2026, 5, 16, 21, 30), Venue: venues[1], Status: MatchStatus.Scheduled, HomeScore: (int?)null, AwayScore: (int?)null)
        };

        var matches = new List<Match>();
        foreach (var seed in seedMatches)
        {
            var match = await db.Matches.FirstOrDefaultAsync(
                x => x.CompetitionId == competitionId
                    && x.RoundId == seed.Round.Id
                    && x.HomeTeamId == seed.Home.Id
                    && x.AwayTeamId == seed.Away.Id
                    && x.StartsAt == seed.StartsAt,
                cancellationToken);

            if (match is null)
            {
                match = new Match(
                    competitionId,
                    seed.Round.Id,
                    seed.Home.Id,
                    seed.Away.Id,
                    seed.StartsAt,
                    seed.Venue.Id,
                    seed.Status,
                    seed.HomeScore,
                    seed.AwayScore);
                db.Matches.Add(match);
            }

            matches.Add(match);
        }

        return matches.ToArray();
    }

    private async Task EnsureMatchEventsAsync(
        IReadOnlyList<Match> matches,
        IReadOnlyList<Team> teams,
        IReadOnlyCollection<Player> players,
        CancellationToken cancellationToken)
    {
        var seedEvents = new[]
        {
            new MatchEvent(matches[0].Id, MatchEventType.KickOff, 0),
            SeedEvent(matches[0].Id, MatchEventType.Goal, 22, teams[0].Id, players, "Opening goal"),
            SeedEvent(matches[0].Id, MatchEventType.Goal, 61, teams[1].Id, players, "Equalizer"),
            SeedEvent(matches[0].Id, MatchEventType.Goal, 78, teams[0].Id, players, "Winning goal"),
            new MatchEvent(matches[0].Id, MatchEventType.FullTime, 90),
            new MatchEvent(matches[1].Id, MatchEventType.KickOff, 0),
            SeedEvent(matches[1].Id, MatchEventType.YellowCard, 34, teams[2].Id, players),
            new MatchEvent(matches[1].Id, MatchEventType.FullTime, 90)
        };

        foreach (var seedEvent in seedEvents)
        {
            var exists = await db.MatchEvents.AnyAsync(
                x => x.MatchId == seedEvent.MatchId
                    && x.Type == seedEvent.Type
                    && x.Minute == seedEvent.Minute
                    && x.TeamId == seedEvent.TeamId
                    && x.PlayerId == seedEvent.PlayerId,
                cancellationToken);

            if (!exists)
            {
                db.MatchEvents.Add(seedEvent);
            }
        }
    }

    private async Task EnsureStandingsAsync(Guid competitionId, IReadOnlyList<Team> teams, CancellationToken cancellationToken)
    {
        var seedStandings = new[]
        {
            (Team: teams[0], Played: 1, Won: 1, Drawn: 0, Lost: 0, GoalsFor: 2, GoalsAgainst: 1, Points: 3),
            (Team: teams[3], Played: 1, Won: 0, Drawn: 1, Lost: 0, GoalsFor: 0, GoalsAgainst: 0, Points: 1),
            (Team: teams[2], Played: 1, Won: 0, Drawn: 1, Lost: 0, GoalsFor: 0, GoalsAgainst: 0, Points: 1),
            (Team: teams[1], Played: 1, Won: 0, Drawn: 0, Lost: 1, GoalsFor: 1, GoalsAgainst: 2, Points: 0)
        };

        foreach (var seed in seedStandings)
        {
            var exists = await db.Standings.AnyAsync(
                x => x.CompetitionId == competitionId && x.TeamId == seed.Team.Id,
                cancellationToken);

            if (!exists)
            {
                db.Standings.Add(new Standing(
                    competitionId,
                    seed.Team.Id,
                    seed.Played,
                    seed.Won,
                    seed.Drawn,
                    seed.Lost,
                    seed.GoalsFor,
                    seed.GoalsAgainst,
                    seed.Points));
            }
        }
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
