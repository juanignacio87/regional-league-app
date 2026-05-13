using RegionalLeagueApp.Domain.Collaboration;
using RegionalLeagueApp.Domain.Common;

namespace RegionalLeagueApp.Domain.Clubs;

public sealed class Club : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string ShortName { get; private set; } = string.Empty;
    public string? PrimaryColor { get; private set; }
    public string? LogoPath { get; private set; }
    public int? FoundedYear { get; private set; }
    public Guid? VenueId { get; private set; }
    public Venue? Venue { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime? ArchivedAt { get; private set; }
    public List<Team> Teams { get; private set; } = [];
    public List<ClubContributor> Contributors { get; private set; } = [];

    private Club()
    {
    }

    public Club(string name, string shortName, string? primaryColor = null, int? foundedYear = null, Guid? venueId = null)
    {
        Name = name;
        ShortName = shortName;
        PrimaryColor = primaryColor;
        FoundedYear = foundedYear;
        VenueId = venueId;
    }

    public void UpdateDetails(string name, string shortName, string? primaryColor, int? foundedYear, Guid? venueId)
    {
        Name = name;
        ShortName = shortName;
        PrimaryColor = primaryColor;
        FoundedYear = foundedYear;
        VenueId = venueId;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        ArchivedAt = isActive ? null : DateTime.UtcNow;
    }

    public void SetLogoPath(string? logoPath)
    {
        LogoPath = string.IsNullOrWhiteSpace(logoPath) ? null : logoPath.Trim();
    }
}
