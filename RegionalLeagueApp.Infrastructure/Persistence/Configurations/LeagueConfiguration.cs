using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionalLeagueApp.Domain.Competitions;

namespace RegionalLeagueApp.Infrastructure.Persistence.Configurations;

public sealed class LeagueConfiguration : IEntityTypeConfiguration<League>
{
    public void Configure(EntityTypeBuilder<League> builder)
    {
        builder.ToTable("leagues");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Region).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Country).HasMaxLength(120).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.ArchivedAt);
        builder.HasIndex(x => new { x.Name, x.Region }).IsUnique();

        builder.HasMany(x => x.Seasons)
            .WithOne(x => x.League)
            .HasForeignKey(x => x.LeagueId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
