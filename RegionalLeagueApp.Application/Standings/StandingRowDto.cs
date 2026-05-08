namespace RegionalLeagueApp.Application.Standings;

public sealed record StandingRowDto(
    int Position,
    Guid CompetitionId,
    string CompetitionName,
    Guid ClubId,
    string ClubName,
    int Played,
    int Won,
    int Drawn,
    int Lost,
    int GoalsFor,
    int GoalsAgainst,
    int GoalDifference,
    int Points);
