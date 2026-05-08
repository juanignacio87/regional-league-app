using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionalLeagueApp.Domain.Matches;

namespace RegionalLeagueApp.Infrastructure.Persistence.Configurations;

public sealed class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.ToTable("matches", table =>
        {
            table.HasCheckConstraint("ck_matches_different_teams", "\"HomeTeamId\" <> \"AwayTeamId\"");
            table.HasCheckConstraint("ck_matches_home_score_non_negative", "\"HomeScore\" IS NULL OR \"HomeScore\" >= 0");
            table.HasCheckConstraint("ck_matches_away_score_non_negative", "\"AwayScore\" IS NULL OR \"AwayScore\" >= 0");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.HasIndex(x => new { x.CompetitionId, x.StartsAt });
        builder.HasIndex(x => new { x.RoundId, x.StartsAt });

        builder.HasOne(x => x.HomeTeam)
            .WithMany()
            .HasForeignKey(x => x.HomeTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AwayTeam)
            .WithMany()
            .HasForeignKey(x => x.AwayTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Venue)
            .WithMany(x => x.Matches)
            .HasForeignKey(x => x.VenueId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.Events)
            .WithOne(x => x.Match)
            .HasForeignKey(x => x.MatchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.UpdateProposals)
            .WithOne(x => x.Match)
            .HasForeignKey(x => x.MatchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
