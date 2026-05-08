using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RegionalFootball.Modules.Clubs;
using RegionalFootball.Modules.Collaboration;
using RegionalFootball.Modules.Competitions;
using RegionalFootball.Modules.Identity;
using RegionalFootball.Modules.Matches;
using RegionalFootball.Modules.Players;
using RegionalFootball.Modules.Standings;

namespace RegionalFootball.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<AppUser, IdentityRole, string>(options)
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
    public DbSet<ClubContributor> ClubContributors => Set<ClubContributor>();
    public DbSet<MatchUpdateProposal> MatchUpdateProposals => Set<MatchUpdateProposal>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Club>(entity =>
        {
            entity.HasIndex(x => x.Name).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(120);
            entity.Property(x => x.ShortName).HasMaxLength(20);
        });

        builder.Entity<Team>()
            .HasOne(x => x.Club)
            .WithMany(x => x.Teams)
            .HasForeignKey(x => x.ClubId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Match>(entity =>
        {
            entity.HasOne(x => x.HomeTeam)
                .WithMany()
                .HasForeignKey(x => x.HomeTeamId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.AwayTeam)
                .WithMany()
                .HasForeignKey(x => x.AwayTeamId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Standing>(entity =>
        {
            entity.HasIndex(x => new { x.CompetitionId, x.TeamId }).IsUnique();
        });

        builder.Entity<ClubContributor>(entity =>
        {
            entity.HasIndex(x => new { x.ClubId, x.UserId }).IsUnique();
        });

        builder.Entity<MatchUpdateProposal>()
            .HasOne(x => x.ProposedByUser)
            .WithMany()
            .HasForeignKey(x => x.ProposedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
