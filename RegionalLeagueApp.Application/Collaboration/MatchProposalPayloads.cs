using RegionalLeagueApp.Domain.Matches;

namespace RegionalLeagueApp.Application.Collaboration;

public sealed record MatchStatusAndResultProposalPayload(
    MatchStatus Status);

public sealed record MatchEventAddProposalPayload(
    MatchEventType EventType,
    int Minute,
    Guid TeamId,
    Guid PlayerId,
    string PlayerName);

public sealed record MatchEventDeleteProposalPayload(
    Guid EventId,
    MatchEventType EventType,
    int Minute,
    Guid? TeamId,
    string PlayerName);
