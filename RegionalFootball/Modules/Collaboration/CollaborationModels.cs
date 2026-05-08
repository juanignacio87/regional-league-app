using RegionalFootball.Modules.Clubs;
using RegionalFootball.Modules.Identity;
using RegionalFootball.Modules.Matches;

namespace RegionalFootball.Modules.Collaboration;

public enum ProposalStatus
{
    Pending,
    Approved,
    Rejected
}

public class ClubContributor
{
    public int Id { get; set; }
    public int ClubId { get; set; }
    public Club? Club { get; set; }
    public string UserId { get; set; } = string.Empty;
    public AppUser? User { get; set; }
    public bool CanReportMatches { get; set; } = true;
}

public class MatchUpdateProposal
{
    public int Id { get; set; }
    public int MatchId { get; set; }
    public Match? Match { get; set; }
    public string ProposedByUserId { get; set; } = string.Empty;
    public AppUser? ProposedByUser { get; set; }
    public MatchStatus? Status { get; set; }
    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }
    public string Notes { get; set; } = string.Empty;
    public ProposalStatus StatusReview { get; set; } = ProposalStatus.Pending;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ReviewedAt { get; set; }
    public string? ReviewedByUserId { get; set; }
}

public class AuditLog
{
    public long Id { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string Details { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
