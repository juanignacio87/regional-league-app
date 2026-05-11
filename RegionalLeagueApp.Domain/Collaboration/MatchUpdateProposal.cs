using RegionalLeagueApp.Domain.Common;
using RegionalLeagueApp.Domain.Identity;
using RegionalLeagueApp.Domain.Matches;

namespace RegionalLeagueApp.Domain.Collaboration;

public sealed class MatchUpdateProposal : Entity
{
    public Guid MatchId { get; private set; }
    public Match? Match { get; private set; }
    public Guid ProposedByUserId { get; private set; }
    public AppUser? ProposedByUser { get; private set; }
    public ProposalChangeType ChangeType { get; private set; } = ProposalChangeType.MatchStatusAndResult;
    public MatchStatus? ProposedStatus { get; private set; }
    public int? ProposedHomeScore { get; private set; }
    public int? ProposedAwayScore { get; private set; }
    public string PayloadJson { get; private set; } = "{}";
    public string? Notes { get; private set; }
    public ProposalStatus Status { get; private set; } = ProposalStatus.Pending;
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ReviewedAt { get; private set; }
    public Guid? ReviewedByUserId { get; private set; }
    public AppUser? ReviewedByUser { get; private set; }

    private MatchUpdateProposal()
    {
    }

    public MatchUpdateProposal(Guid matchId, Guid proposedByUserId)
    {
        MatchId = matchId;
        ProposedByUserId = proposedByUserId;
    }

    public static MatchUpdateProposal ForMatchStatusAndResult(
        Guid matchId,
        Guid proposedByUserId,
        MatchStatus proposedStatus,
        int? proposedHomeScore,
        int? proposedAwayScore,
        string payloadJson)
    {
        return new MatchUpdateProposal(matchId, proposedByUserId)
        {
            ChangeType = ProposalChangeType.MatchStatusAndResult,
            ProposedStatus = proposedStatus,
            ProposedHomeScore = proposedHomeScore,
            ProposedAwayScore = proposedAwayScore,
            PayloadJson = payloadJson
        };
    }

    public static MatchUpdateProposal ForMatchEvent(
        Guid matchId,
        Guid proposedByUserId,
        ProposalChangeType changeType,
        string payloadJson)
    {
        if (changeType is not (ProposalChangeType.MatchEventAdd or ProposalChangeType.MatchEventDelete))
        {
            throw new ArgumentOutOfRangeException(nameof(changeType), "Only match event proposal types are supported.");
        }

        return new MatchUpdateProposal(matchId, proposedByUserId)
        {
            ChangeType = changeType,
            PayloadJson = payloadJson
        };
    }

    public void Approve(Guid reviewedByUserId)
    {
        Status = ProposalStatus.Approved;
        ReviewedByUserId = reviewedByUserId;
        ReviewedAt = DateTimeOffset.UtcNow;
    }

    public void Reject(Guid reviewedByUserId)
    {
        Status = ProposalStatus.Rejected;
        ReviewedByUserId = reviewedByUserId;
        ReviewedAt = DateTimeOffset.UtcNow;
    }
}
