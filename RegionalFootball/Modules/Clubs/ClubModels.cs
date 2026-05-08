using RegionalFootball.Modules.Players;

namespace RegionalFootball.Modules.Clubs;

public class Club
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    public string PrimaryColor { get; set; } = "#0f766e";
    public string SecondaryColor { get; set; } = "#f8fafc";
    public int FoundedYear { get; set; }
    public int? VenueId { get; set; }
    public Venue? Venue { get; set; }
    public List<Team> Teams { get; set; } = [];
}

public class Venue
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public int Capacity { get; set; }
}

public class Team
{
    public int Id { get; set; }
    public int ClubId { get; set; }
    public Club? Club { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = "Primera";
    public List<Player> Players { get; set; } = [];
}
