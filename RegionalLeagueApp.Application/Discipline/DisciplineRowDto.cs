namespace RegionalLeagueApp.Application.Discipline;

public sealed record DisciplineRowDto(
    Guid CompetitionId,
    string CompetitionName,
    Guid? PlayerId,
    string PlayerName,
    Guid TeamId,
    string TeamName,
    Guid ClubId,
    string ClubName,
    int YellowCards,
    int RedCards,
    DisciplineStatus Status);
