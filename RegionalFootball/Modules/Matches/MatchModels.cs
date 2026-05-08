using RegionalFootball.Modules.Clubs;
using RegionalFootball.Modules.Competitions;
using RegionalFootball.Modules.Players;

namespace RegionalFootball.Modules.Matches;

public enum MatchStatus
{
    Scheduled,
    Live,
    HalfTime,
    Finished,
    Postponed,
    Suspended,
    Cancelled
}

public enum MatchEventType
{
    KickOff,
    Goal,
    YellowCard,
    RedCard,
    HalfTime,
    SecondHalfStart,
    FullTime,
    Substitution,
    PenaltyScored,
    PenaltyMissed,
    OwnGoal
}

public class Match
{
    public int Id { get; set; }
    public int RoundId { get; set; }
    public Round? Round { get; set; }
    public int HomeTeamId { get; set; }
    public Team? HomeTeam { get; set; }
    public int AwayTeamId { get; set; }
    public Team? AwayTeam { get; set; }
    public int? VenueId { get; set; }
    public Venue? Venue { get; set; }
    public DateTimeOffset StartsAt { get; set; }
    public MatchStatus Status { get; set; }
    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }
    public List<MatchEvent> Events { get; set; } = [];
}

public class MatchEvent
{
    public int Id { get; set; }
    public int MatchId { get; set; }
    public Match? Match { get; set; }
    public MatchEventType Type { get; set; }
    public int Minute { get; set; }
    public int? TeamId { get; set; }
    public Team? Team { get; set; }
    public int? PlayerId { get; set; }
    public Player? Player { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
