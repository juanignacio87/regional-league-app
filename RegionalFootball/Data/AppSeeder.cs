using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RegionalFootball.Modules.Clubs;
using RegionalFootball.Modules.Competitions;
using RegionalFootball.Modules.Identity;
using RegionalFootball.Modules.Matches;
using RegionalFootball.Modules.Players;
using RegionalFootball.Modules.Standings;

namespace RegionalFootball.Data;

public class AppSeeder(
    ApplicationDbContext db,
    RoleManager<IdentityRole> roleManager,
    UserManager<ApplicationUser> userManager,
    StandingService standingService,
    IWebHostEnvironment environment)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await db.Database.MigrateAsync(cancellationToken);

        foreach (var role in AppRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        await EnsureAdminAsync();

        if (await db.Leagues.AnyAsync(cancellationToken))
        {
            return;
        }

        var league = new League { Name = "Liga Regional Norte", Region = "Norte", Country = "Argentina" };
        var season = new Season
        {
            League = league,
            Name = "Temporada 2026",
            StartsOn = new DateOnly(2026, 3, 1),
            EndsOn = new DateOnly(2026, 11, 30),
            IsActive = true
        };
        var competition = new Competition { Season = season, Name = "Primera Division", Format = "League" };
        var round1 = new Round { Competition = competition, Name = "Fecha 1", SortOrder = 1 };
        var round2 = new Round { Competition = competition, Name = "Fecha 2", SortOrder = 2 };

        var venues = new[]
        {
            new Venue { Name = "Estadio Municipal", City = "San Pedro", Capacity = 4200 },
            new Venue { Name = "Cancha del Barrio", City = "Villa Norte", Capacity = 1800 },
            new Venue { Name = "Parque Central", City = "Rivadavia", Capacity = 2600 },
            new Venue { Name = "La Fortaleza", City = "Belgrano", Capacity = 3100 }
        };

        var teams = new[]
        {
            CreateTeam("Atletico San Pedro", "ASP", "#166534", 1948, venues[0]),
            CreateTeam("Deportivo Norte", "DN", "#1d4ed8", 1972, venues[1]),
            CreateTeam("Union Rivadavia", "UR", "#b91c1c", 1965, venues[2]),
            CreateTeam("Club Belgrano", "CB", "#7c2d12", 1954, venues[3])
        };

        db.AddRange(round1, round2);
        db.Teams.AddRange(teams);
        db.Matches.AddRange(
            new Match { Round = round1, HomeTeam = teams[0], AwayTeam = teams[1], Venue = venues[0], StartsAt = new DateTimeOffset(2026, 5, 2, 16, 0, 0, TimeSpan.FromHours(-3)), Status = MatchStatus.Finished, HomeScore = 2, AwayScore = 1 },
            new Match { Round = round1, HomeTeam = teams[2], AwayTeam = teams[3], Venue = venues[2], StartsAt = new DateTimeOffset(2026, 5, 2, 18, 30, 0, TimeSpan.FromHours(-3)), Status = MatchStatus.Finished, HomeScore = 0, AwayScore = 0 },
            new Match { Round = round2, HomeTeam = teams[1], AwayTeam = teams[2], Venue = venues[1], StartsAt = new DateTimeOffset(2026, 5, 9, 16, 0, 0, TimeSpan.FromHours(-3)), Status = MatchStatus.Scheduled },
            new Match { Round = round2, HomeTeam = teams[3], AwayTeam = teams[0], Venue = venues[3], StartsAt = new DateTimeOffset(2026, 5, 9, 18, 30, 0, TimeSpan.FromHours(-3)), Status = MatchStatus.Scheduled });

        await db.SaveChangesAsync(cancellationToken);
        await standingService.RebuildCompetitionAsync(competition.Id, cancellationToken);
    }

    private async Task EnsureAdminAsync()
    {
        const string email = "admin@regional.test";
        var admin = await userManager.FindByEmailAsync(email);

        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                DisplayName = "Admin Regional"
            };
            var password = environment.IsDevelopment() ? "Admin123!" : Guid.NewGuid().ToString("N") + "aA1!";
            await userManager.CreateAsync(admin, password);
        }

        if (!await userManager.IsInRoleAsync(admin, AppRoles.Admin))
        {
            await userManager.AddToRoleAsync(admin, AppRoles.Admin);
        }
    }

    private static Team CreateTeam(string clubName, string shortName, string color, int foundedYear, Venue venue)
    {
        var club = new Club
        {
            Name = clubName,
            ShortName = shortName,
            PrimaryColor = color,
            FoundedYear = foundedYear,
            Venue = venue
        };

        var team = new Team { Club = club, Name = $"{shortName} Primera", Category = "Primera" };
        team.Players.AddRange(new[]
        {
            new Player { FirstName = "Mateo", LastName = shortName, ShirtNumber = 1, Position = "Arquero" },
            new Player { FirstName = "Tomas", LastName = shortName, ShirtNumber = 5, Position = "Defensor" },
            new Player { FirstName = "Lucas", LastName = shortName, ShirtNumber = 8, Position = "Mediocampista" },
            new Player { FirstName = "Nicolas", LastName = shortName, ShirtNumber = 9, Position = "Delantero" }
        });

        return team;
    }
}
