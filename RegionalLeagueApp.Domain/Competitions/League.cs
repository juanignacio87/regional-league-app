using RegionalLeagueApp.Domain.Common;

namespace RegionalLeagueApp.Domain.Competitions;

public sealed class League : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string Region { get; private set; } = string.Empty;
    public string Country { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public DateTime? ArchivedAt { get; private set; }
    public List<Season> Seasons { get; private set; } = [];

    private League()
    {
    }

    public League(string name, string region, string country)
    {
        Name = name;
        Region = region;
        Country = country;
    }

    public void UpdateDetails(string name, string region, string country)
    {
        Name = name;
        Region = region;
        Country = country;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        ArchivedAt = isActive ? null : DateTime.UtcNow;
    }
}
