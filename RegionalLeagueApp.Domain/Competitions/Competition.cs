using RegionalLeagueApp.Domain.Clubs;
using RegionalLeagueApp.Domain.Common;
using RegionalLeagueApp.Domain.Matches;
using RegionalLeagueApp.Domain.Standings;

namespace RegionalLeagueApp.Domain.Competitions;

public sealed class Competition : Entity
{
    public Guid SeasonId { get; private set; }
    public Season? Season { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Format { get; private set; } = "League";
    public bool IsActive { get; private set; } = true;
    public DateTime? ArchivedAt { get; private set; }
    public List<Team> Teams { get; private set; } = [];
    public List<Round> Rounds { get; private set; } = [];
    public List<Match> Matches { get; private set; } = [];
    public List<Standing> Standings { get; private set; } = [];

    private Competition()
    {
    }

    public Competition(Guid seasonId, string name, string format = "League")
    {
        SeasonId = seasonId;
        Name = name;
        Format = format;
    }

    public void UpdateDetails(string name, string format)
    {
        Name = name;
        Format = format;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        ArchivedAt = isActive ? null : DateTime.UtcNow;
    }
}
