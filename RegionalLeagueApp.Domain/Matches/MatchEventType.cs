namespace RegionalLeagueApp.Domain.Matches;

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
