using RegionalLeagueApp.Domain.Clubs;
using RegionalLeagueApp.Domain.Common;
using RegionalLeagueApp.Domain.Matches;

namespace RegionalLeagueApp.Domain.Players;

public sealed class Player : Entity
{
    public Guid TeamId { get; private set; }
    public Team? Team { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public int? ShirtNumber { get; private set; }
    public PlayerPosition Position { get; private set; }
    public DateOnly? BirthDate { get; private set; }
    public List<MatchEvent> MatchEvents { get; private set; } = [];

    public string FullName => $"{FirstName} {LastName}".Trim();

    private Player()
    {
    }

    public Player(Guid teamId, string firstName, string lastName, PlayerPosition position, int? shirtNumber = null, DateOnly? birthDate = null)
    {
        TeamId = teamId;
        FirstName = firstName;
        LastName = lastName;
        Position = position;
        ShirtNumber = shirtNumber;
        BirthDate = birthDate;
    }
}
