using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionalLeagueApp.Domain.Competitions;

namespace RegionalLeagueApp.Infrastructure.Persistence.Configurations;

public sealed class CompetitionConfiguration : IEntityTypeConfiguration<Competition>
{
    public void Configure(EntityTypeBuilder<Competition> builder)
    {
        builder.ToTable("competitions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Format).HasMaxLength(50).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.ArchivedAt);
        builder.HasIndex(x => new { x.SeasonId, x.Name }).IsUnique();

        builder.HasMany(x => x.Teams)
            .WithOne(x => x.Competition)
            .HasForeignKey(x => x.CompetitionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Rounds)
            .WithOne(x => x.Competition)
            .HasForeignKey(x => x.CompetitionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Matches)
            .WithOne(x => x.Competition)
            .HasForeignKey(x => x.CompetitionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Standings)
            .WithOne(x => x.Competition)
            .HasForeignKey(x => x.CompetitionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
