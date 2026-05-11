using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionalLeagueApp.Domain.Clubs;

namespace RegionalLeagueApp.Infrastructure.Persistence.Configurations;

public sealed class ClubConfiguration : IEntityTypeConfiguration<Club>
{
    public void Configure(EntityTypeBuilder<Club> builder)
    {
        builder.ToTable("clubs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.ShortName).HasMaxLength(30).IsRequired();
        builder.Property(x => x.PrimaryColor).HasMaxLength(20);
        builder.Property(x => x.LogoPath).HasMaxLength(300);
        builder.HasIndex(x => x.Name).IsUnique();
        builder.HasIndex(x => x.ShortName).IsUnique();

        builder.HasOne(x => x.Venue)
            .WithMany(x => x.Clubs)
            .HasForeignKey(x => x.VenueId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.Teams)
            .WithOne(x => x.Club)
            .HasForeignKey(x => x.ClubId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Contributors)
            .WithOne(x => x.Club)
            .HasForeignKey(x => x.ClubId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
