using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionalLeagueApp.Domain.Standings;

namespace RegionalLeagueApp.Infrastructure.Persistence.Configurations;

public sealed class StandingConfiguration : IEntityTypeConfiguration<Standing>
{
    public void Configure(EntityTypeBuilder<Standing> builder)
    {
        builder.ToTable("standings", table =>
            table.HasCheckConstraint(
                "ck_standings_values_non_negative",
                "\"Played\" >= 0 AND \"Won\" >= 0 AND \"Drawn\" >= 0 AND \"Lost\" >= 0 AND \"GoalsFor\" >= 0 AND \"GoalsAgainst\" >= 0 AND \"Points\" >= 0"));
        builder.HasKey(x => x.Id);
        builder.Ignore(x => x.GoalDifference);
        builder.HasIndex(x => new { x.CompetitionId, x.TeamId }).IsUnique();

        builder.HasOne(x => x.Team)
            .WithMany()
            .HasForeignKey(x => x.TeamId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
