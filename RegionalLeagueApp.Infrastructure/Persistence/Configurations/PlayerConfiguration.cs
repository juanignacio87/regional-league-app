using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionalLeagueApp.Domain.Players;

namespace RegionalLeagueApp.Infrastructure.Persistence.Configurations;

public sealed class PlayerConfiguration : IEntityTypeConfiguration<Player>
{
    public void Configure(EntityTypeBuilder<Player> builder)
    {
        builder.ToTable("players");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.LastName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(160).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.ArchivedAt);
        builder.Property(x => x.Position).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.HasIndex(x => new { x.TeamId, x.ShirtNumber }).IsUnique();
        builder.HasIndex(x => x.TeamId);
        builder.HasIndex(x => new { x.TeamId, x.IsActive });
    }
}
