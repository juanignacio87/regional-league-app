using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RegionalLeagueApp.Application.Abstractions.Data;
using RegionalLeagueApp.Domain.Clubs;
using RegionalLeagueApp.Domain.Collaboration;
using RegionalLeagueApp.Domain.Competitions;
using RegionalLeagueApp.Domain.Identity;
using RegionalLeagueApp.Infrastructure.Identity;
using RegionalLeagueApp.Domain.Matches;
using RegionalLeagueApp.Domain.Players;
using RegionalLeagueApp.Domain.Standings;

namespace RegionalLeagueApp.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationIdentityUser, IdentityRole<Guid>, Guid>(options), IApplicationDbContext
{
    public DbSet<League> Leagues => Set<League>();
    public DbSet<Season> Seasons => Set<Season>();
    public DbSet<Competition> Competitions => Set<Competition>();
    public DbSet<Round> Rounds => Set<Round>();
    public DbSet<Club> Clubs => Set<Club>();
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<MatchEvent> MatchEvents => Set<MatchEvent>();
    public DbSet<Standing> Standings => Set<Standing>();
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<ClubContributor> ClubContributors => Set<ClubContributor>();
    public DbSet<MatchUpdateProposal> MatchUpdateProposals => Set<MatchUpdateProposal>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    IQueryable<League> IApplicationDbContext.Leagues => Leagues.AsNoTracking();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
