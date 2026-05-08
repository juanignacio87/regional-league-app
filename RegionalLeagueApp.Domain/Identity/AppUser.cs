using RegionalLeagueApp.Domain.Collaboration;
using RegionalLeagueApp.Domain.Common;

namespace RegionalLeagueApp.Domain.Identity;

// Domain user profile. Authentication storage can be wired later in Infrastructure.
public sealed class AppUser : Entity
{
    public string Email { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public List<ClubContributor> ClubContributions { get; private set; } = [];
    public List<MatchUpdateProposal> MatchUpdateProposals { get; private set; } = [];

    private AppUser()
    {
    }

    public AppUser(string email, string displayName)
    {
        Email = email;
        DisplayName = displayName;
    }

    public AppUser(Guid id, string email, string displayName)
        : this(email, displayName)
    {
        Id = id;
    }
}
