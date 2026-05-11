using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionalLeagueApp.Domain.Collaboration;

namespace RegionalLeagueApp.Infrastructure.Persistence.Configurations;

public sealed class ClubContributorConfiguration : IEntityTypeConfiguration<ClubContributor>
{
    public void Configure(EntityTypeBuilder<ClubContributor> builder)
    {
        builder.ToTable("club_contributors");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.TeamId);
        builder.HasIndex(x => new { x.UserId, x.TeamId })
            .IsUnique()
            .HasFilter("\"TeamId\" IS NOT NULL");

        builder.HasOne(x => x.Team)
            .WithMany()
            .HasForeignKey(x => x.TeamId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
