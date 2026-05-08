using RegionalLeagueApp.Domain.Common;

namespace RegionalLeagueApp.Domain.Competitions;

public sealed class Season : Entity
{
    public Guid LeagueId { get; private set; }
    public League? League { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DateOnly StartsOn { get; private set; }
    public DateOnly EndsOn { get; private set; }
    public bool IsActive { get; private set; }
    public List<Competition> Competitions { get; private set; } = [];

    private Season()
    {
    }

    public Season(Guid leagueId, string name, DateOnly startsOn, DateOnly endsOn, bool isActive = false)
    {
        LeagueId = leagueId;
        Name = name;
        StartsOn = startsOn;
        EndsOn = endsOn;
        IsActive = isActive;
    }
}
