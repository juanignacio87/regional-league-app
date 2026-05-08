using Microsoft.EntityFrameworkCore;
using RegionalFootball.Data;
using RegionalFootball.Modules.Matches;
using RegionalFootball.Modules.Standings;

namespace RegionalFootball.Modules.Collaboration;

public class ProposalService(ApplicationDbContext db, StandingService standingService)
{
    public async Task<int> SubmitMatchResultAsync(int matchId, string userId, int homeScore, int awayScore, string notes, CancellationToken cancellationToken = default)
    {
        var proposal = new MatchUpdateProposal
        {
            MatchId = matchId,
            ProposedByUserId = userId,
            HomeScore = homeScore,
            AwayScore = awayScore,
            Status = MatchStatus.Finished,
            Notes = notes,
        };

        db.MatchUpdateProposals.Add(proposal);
        await db.SaveChangesAsync(cancellationToken);
        return proposal.Id;
    }

    public async Task ApproveAsync(int proposalId, string moderatorUserId, CancellationToken cancellationToken = default)
    {
        var proposal = await db.MatchUpdateProposals
            .Include(x => x.Match)
            .ThenInclude(x => x!.Round)
            .FirstAsync(x => x.Id == proposalId, cancellationToken);

        if (proposal.Match is null)
        {
            throw new InvalidOperationException("Proposal has no match.");
        }

        proposal.Match.HomeScore = proposal.HomeScore;
        proposal.Match.AwayScore = proposal.AwayScore;
        proposal.Match.Status = proposal.Status ?? MatchStatus.Finished;
        proposal.StatusReview = ProposalStatus.Approved;
        proposal.ReviewedByUserId = moderatorUserId;
        proposal.ReviewedAt = DateTimeOffset.UtcNow;

        db.AuditLogs.Add(new AuditLog
        {
            EntityName = nameof(Match),
            EntityId = proposal.MatchId.ToString(),
            Action = "ApproveProposal",
            UserId = moderatorUserId,
            Details = $"Proposal {proposal.Id} approved: {proposal.HomeScore}-{proposal.AwayScore}"
        });

        await db.SaveChangesAsync(cancellationToken);

        if (proposal.Match.Round is not null)
        {
            await standingService.RebuildCompetitionAsync(proposal.Match.Round.CompetitionId, cancellationToken);
        }
    }

    public async Task RejectAsync(int proposalId, string moderatorUserId, CancellationToken cancellationToken = default)
    {
        var proposal = await db.MatchUpdateProposals.FirstAsync(x => x.Id == proposalId, cancellationToken);
        proposal.StatusReview = ProposalStatus.Rejected;
        proposal.ReviewedByUserId = moderatorUserId;
        proposal.ReviewedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }
}
