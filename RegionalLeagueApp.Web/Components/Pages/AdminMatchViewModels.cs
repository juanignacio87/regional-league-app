using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using RegionalLeagueApp.Domain.Matches;

namespace RegionalLeagueApp.Web.Components.Pages;

public sealed record AdminMatchRow(
    Guid Id,
    Guid CompetitionId,
    Guid LeagueId,
    string LeagueName,
    string SeasonName,
    string CompetitionName,
    Guid RoundId,
    string RoundName,
    Guid HomeTeamId,
    string HomeTeamName,
    Guid AwayTeamId,
    string AwayTeamName,
    DateTimeOffset StartsAt,
    MatchStatus Status,
    int? HomeScore,
    int? AwayScore);

public sealed class AdminMatchEditModel
{
    public Guid MatchId { get; set; }
    public Guid CompetitionId { get; set; }
    public Guid LeagueId { get; set; }
    public string LeagueName { get; set; } = string.Empty;
    public string SeasonName { get; set; } = string.Empty;
    public string CompetitionName { get; set; } = string.Empty;
    public Guid RoundId { get; set; }
    public string RoundName { get; set; } = string.Empty;
    public Guid HomeTeamId { get; set; }
    public string HomeTeamName { get; set; } = string.Empty;
    public Guid AwayTeamId { get; set; }
    public string AwayTeamName { get; set; } = string.Empty;
    public DateTimeOffset StartsAt { get; set; }
    public string StartsAtInput { get; set; } = string.Empty;
    public MatchStatus Status { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Los goles local no pueden ser negativos.")]
    public int? HomeScore { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Los goles visitante no pueden ser negativos.")]
    public int? AwayScore { get; set; }
}

public sealed record AdminMatchEventRow(
    Guid Id,
    MatchEventType EventType,
    int Minute,
    Guid? TeamId,
    string TeamName,
    Guid? PlayerId,
    string PlayerName,
    string? Notes);

public sealed record EventPlayerOption(
    Guid Id,
    Guid TeamId,
    string DisplayName,
    int? ShirtNumber,
    string Label);

public sealed record PendingProposalRow(
    Guid Id,
    string MatchLabel,
    string ProposedBy,
    DateTimeOffset CreatedAt,
    string Description);

public sealed class EventCreateModel
{
    public MatchEventType EventType { get; set; } = MatchEventType.Goal;
    public Guid TeamId { get; set; }

    [Required(ErrorMessage = "El nombre del jugador es obligatorio.")]
    public string PlayerName { get; set; } = string.Empty;

    public Guid? PlayerId { get; set; }

    [Range(0, 130, ErrorMessage = "El minuto debe estar entre 0 y 130.")]
    public int Minute { get; set; }

    public string? Notes { get; set; }
}

public sealed class EventEditModel
{
    public MatchEventType EventType { get; set; }
    public int Minute { get; set; }
    public Guid TeamId { get; set; }
    public Guid? PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public sealed record EventPlayerChangedArgs(Guid EventId, EventEditModel Edit, ChangeEventArgs Change);
