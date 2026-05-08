using RegionalFootball.Modules.Clubs;

namespace RegionalFootball.Modules.Players;

public class Player
{
    public int Id { get; set; }
    public int TeamId { get; set; }
    public Team? Team { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int ShirtNumber { get; set; }
    public string Position { get; set; } = string.Empty;
    public DateOnly? BirthDate { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();
}
