namespace RegionalLeagueApp.Application.Standings;

public sealed record StandingRowDto(
    int Position,
    Guid CompetitionId,
    string CompetitionName,
    Guid TeamId,
    Guid ClubId,
    string ClubName,
    string? ClubLogoPath,
    int Played,
    int Won,
    int Drawn,
    int Lost,
    int GoalsFor,
    int GoalsAgainst,
    int GoalDifference,
    int Points);
