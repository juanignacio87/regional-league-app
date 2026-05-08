using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionalLeagueApp.Domain.Competitions;

namespace RegionalLeagueApp.Infrastructure.Persistence.Configurations;

public sealed class RoundConfiguration : IEntityTypeConfiguration<Round>
{
    public void Configure(EntityTypeBuilder<Round> builder)
    {
        builder.ToTable("rounds");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => new { x.CompetitionId, x.SortOrder }).IsUnique();
        builder.HasIndex(x => new { x.CompetitionId, x.Name }).IsUnique();

        builder.HasMany(x => x.Matches)
            .WithOne(x => x.Round)
            .HasForeignKey(x => x.RoundId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
