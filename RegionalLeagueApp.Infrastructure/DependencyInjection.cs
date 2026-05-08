using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RegionalLeagueApp.Application.Abstractions.Data;
using RegionalLeagueApp.Application.Standings;
using RegionalLeagueApp.Infrastructure.Identity;
using RegionalLeagueApp.Infrastructure.Persistence;
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
        services.AddScoped<IStandingsQueryService, EfStandingsQueryService>();
        services.Configure<DevelopmentSeedOptions>(options =>
        {
            var section = configuration.GetSection("Seed");
            options.Enabled = bool.TryParse(section["Enabled"], out var enabled) && enabled;
            options.AdminEmail = section["AdminEmail"] ?? options.AdminEmail;
            options.AdminDisplayName = section["AdminDisplayName"] ?? options.AdminDisplayName;
            options.AdminPassword = section["AdminPassword"] ?? options.AdminPassword;
        });
        services.AddScoped<DevelopmentDataSeeder>();

        return services;
    }
}
