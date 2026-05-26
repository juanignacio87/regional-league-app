using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RegionalLeagueApp.Application.Abstractions.Data;
using RegionalLeagueApp.Application.Collaboration;
using RegionalLeagueApp.Application.Discipline;
using RegionalLeagueApp.Application.Fixtures;
using RegionalLeagueApp.Infrastructure.Collaboration;
using RegionalLeagueApp.Application.Matches;
using RegionalLeagueApp.Application.Players;
using RegionalLeagueApp.Application.Standings;
using RegionalLeagueApp.Infrastructure.Fixtures;
using RegionalLeagueApp.Infrastructure.Discipline;
using RegionalLeagueApp.Infrastructure.Identity;
using RegionalLeagueApp.Infrastructure.Matches;
using RegionalLeagueApp.Infrastructure.Persistence;
using RegionalLeagueApp.Infrastructure.Players;
using RegionalLeagueApp.Infrastructure.Seed;
using RegionalLeagueApp.Infrastructure.Standings;

namespace RegionalLeagueApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured. Set ConnectionStrings__DefaultConnection as an environment variable or use the DevelopmentLocal profile.");
        }

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddDbContextFactory<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)),
            ServiceLifetime.Scoped);

        services.AddIdentityCore<ApplicationIdentityUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IMatchAdministrationPermissionService, EfMatchAdministrationPermissionService>();
        services.AddScoped<IMatchScoreRecalculationService, EfMatchScoreRecalculationService>();
        services.AddScoped<IFixtureCsvImportService, EfFixtureCsvImportService>();
        services.AddScoped<IFixtureGeneratorService, FixtureGeneratorService>();
        services.AddScoped<IFixtureRulesService, EfFixtureRulesService>();
        services.AddScoped<IPlayerCsvImportService, EfPlayerCsvImportService>();
        services.AddScoped<IDisciplineQueryService, EfDisciplineQueryService>();
        services.AddScoped<IStandingsQueryService, EfStandingsQueryService>();
        services.AddScoped<IStandingsRecalculationService, EfStandingsRecalculationService>();
        services.Configure<DevelopmentSeedOptions>(options =>
        {
            var section = configuration.GetSection("Seed");
            options.Enabled = bool.TryParse(section["Enabled"], out var enabled) && enabled;
            options.AdminEmail = section["AdminEmail"] ?? options.AdminEmail;
            options.AdminDisplayName = section["AdminDisplayName"] ?? options.AdminDisplayName;
            options.AdminPassword = section["AdminPassword"] ?? options.AdminPassword;
            options.ModeratorEmail = section["ModeratorEmail"] ?? options.ModeratorEmail;
            options.ModeratorDisplayName = section["ModeratorDisplayName"] ?? options.ModeratorDisplayName;
            options.ModeratorPassword = section["ModeratorPassword"] ?? options.ModeratorPassword;
            options.ContributorEmail = section["ContributorEmail"] ?? options.ContributorEmail;
            options.ContributorDisplayName = section["ContributorDisplayName"] ?? options.ContributorDisplayName;
            options.ContributorPassword = section["ContributorPassword"] ?? options.ContributorPassword;
        });
        services.AddScoped<DevelopmentDataSeeder>();

        return services;
    }
}
