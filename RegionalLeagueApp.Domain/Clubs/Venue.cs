using RegionalLeagueApp.Domain.Common;
using RegionalLeagueApp.Domain.Matches;

namespace RegionalLeagueApp.Domain.Clubs;

public sealed class Venue : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public int? Capacity { get; private set; }
    public List<Club> Clubs { get; private set; } = [];
    public List<Match> Matches { get; private set; } = [];

    private Venue()
    {
    }

    public Venue(string name, string city, int? capacity = null)
    {
        Name = name;
        City = city;
        Capacity = capacity;
    }
}
