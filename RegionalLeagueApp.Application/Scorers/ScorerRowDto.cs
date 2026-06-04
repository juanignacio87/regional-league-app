namespace RegionalLeagueApp.Application.Scorers;

public sealed record ScorerRowDto(
    Guid? PlayerId,
    string PlayerName,
    Guid TeamId,
    string TeamName,
    Guid CompetitionId,
    string CompetitionName,
    int Goals);
