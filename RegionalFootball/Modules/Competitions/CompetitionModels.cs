namespace RegionalFootball.Modules.Competitions;

public class League
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public List<Season> Seasons { get; set; } = [];
}

public class Season
{
    public int Id { get; set; }
    public int LeagueId { get; set; }
    public League? League { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly StartsOn { get; set; }
    public DateOnly EndsOn { get; set; }
    public bool IsActive { get; set; }
    public List<Competition> Competitions { get; set; } = [];
}

public class Competition
{
    public int Id { get; set; }
    public int SeasonId { get; set; }
    public Season? Season { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Format { get; set; } = "League";
    public List<Round> Rounds { get; set; } = [];
}

public class Round
{
    public int Id { get; set; }
    public int CompetitionId { get; set; }
    public Competition? Competition { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}
