using RegionalLeagueApp.Domain.Clubs;
using RegionalLeagueApp.Domain.Common;
using RegionalLeagueApp.Domain.Identity;

namespace RegionalLeagueApp.Domain.Collaboration;

public sealed class ClubContributor : Entity
{
    public Guid ClubId { get; private set; }
    public Club? Club { get; private set; }
    public Guid UserId { get; private set; }
    public AppUser? User { get; private set; }
    public bool CanReportMatches { get; private set; } = true;
    public bool CanReportPlayers { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    private ClubContributor()
    {
    }

    public ClubContributor(Guid clubId, Guid userId, bool canReportMatches = true)
    {
        ClubId = clubId;
        UserId = userId;
        CanReportMatches = canReportMatches;
    }
}
