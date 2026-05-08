using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionalLeagueApp.Domain.Collaboration;

namespace RegionalLeagueApp.Infrastructure.Persistence.Configurations;

public sealed class MatchUpdateProposalConfiguration : IEntityTypeConfiguration<MatchUpdateProposal>
{
    public void Configure(EntityTypeBuilder<MatchUpdateProposal> builder)
    {
        builder.ToTable("match_update_proposals", table =>
        {
            table.HasCheckConstraint("ck_match_update_proposals_home_score_non_negative", "\"ProposedHomeScore\" IS NULL OR \"ProposedHomeScore\" >= 0");
            table.HasCheckConstraint("ck_match_update_proposals_away_score_non_negative", "\"ProposedAwayScore\" IS NULL OR \"ProposedAwayScore\" >= 0");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ProposedStatus).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.HasIndex(x => new { x.MatchId, x.Status });
        builder.HasIndex(x => x.ProposedByUserId);

        builder.HasOne(x => x.ReviewedByUser)
            .WithMany()
            .HasForeignKey(x => x.ReviewedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
