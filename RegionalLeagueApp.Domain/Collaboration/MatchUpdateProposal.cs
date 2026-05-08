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
    public MatchStatus? ProposedStatus { get; private set; }
    public int? ProposedHomeScore { get; private set; }
    public int? ProposedAwayScore { get; private set; }
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
}
