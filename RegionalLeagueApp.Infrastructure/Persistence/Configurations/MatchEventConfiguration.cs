using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionalLeagueApp.Domain.Matches;

namespace RegionalLeagueApp.Infrastructure.Persistence.Configurations;

public sealed class MatchEventConfiguration : IEntityTypeConfiguration<MatchEvent>
{
    public void Configure(EntityTypeBuilder<MatchEvent> builder)
    {
        builder.ToTable("match_events", table =>
            table.HasCheckConstraint("ck_match_events_minute_non_negative", "\"Minute\" >= 0"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(500);
        builder.HasIndex(x => new { x.MatchId, x.Minute });

        builder.HasOne(x => x.Team)
            .WithMany()
            .HasForeignKey(x => x.TeamId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Player)
            .WithMany(x => x.MatchEvents)
            .HasForeignKey(x => x.PlayerId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
