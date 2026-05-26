using RegionalLeagueApp.Domain.Matches;

namespace RegionalLeagueApp.Web.Components.Pages;

public sealed record CompetitionItem(Guid Id, string Name, string SeasonName, string Format, int TeamCount);

public sealed record ParticipantItem(Guid ClubId, string ClubName, string? LogoPath, string TeamName);

public sealed record ClubOption(Guid Id, string Name);

public sealed record CompetitionOption(Guid Id, string Label);

public sealed record TeamOption(Guid Id, string Label);

public sealed class MatchEditModel
{
    public MatchEditModel(string roundName, string startsAt, Guid homeTeamId, Guid awayTeamId, MatchStatus status)
    {
        RoundName = roundName;
        StartsAt = startsAt;
        HomeTeamId = homeTeamId;
        AwayTeamId = awayTeamId;
        Status = status;
    }

    public string RoundName { get; set; }
    public string StartsAt { get; set; }
    public Guid HomeTeamId { get; set; }
    public Guid AwayTeamId { get; set; }
    public MatchStatus Status { get; set; }
}

public sealed class SeasonEditModel
{
    public SeasonEditModel()
    {
    }

    public SeasonEditModel(string name, string startsOn, string endsOn)
    {
        Name = name;
        StartsOn = startsOn;
        EndsOn = endsOn;
    }

    public string Name { get; set; } = string.Empty;
    public string StartsOn { get; set; } = string.Empty;
    public string EndsOn { get; set; } = string.Empty;
}

public sealed class CompetitionEditModel
{
    public CompetitionEditModel()
    {
    }

    public CompetitionEditModel(string name, string format)
    {
        Name = name;
        Format = format;
    }

    public string Name { get; set; } = string.Empty;
    public string Format { get; set; } = "League";
}
