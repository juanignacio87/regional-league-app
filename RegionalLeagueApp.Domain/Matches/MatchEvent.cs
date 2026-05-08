using RegionalLeagueApp.Domain.Clubs;
using RegionalLeagueApp.Domain.Common;
using RegionalLeagueApp.Domain.Players;

namespace RegionalLeagueApp.Domain.Matches;

public sealed class MatchEvent : Entity
{
    public Guid MatchId { get; private set; }
    public Match? Match { get; private set; }
    public MatchEventType Type { get; private set; }
    public int Minute { get; private set; }
    public Guid? TeamId { get; private set; }
    public Team? Team { get; private set; }
    public Guid? PlayerId { get; private set; }
    public Player? Player { get; private set; }
    public string? Notes { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    private MatchEvent()
    {
    }

    public MatchEvent(Guid matchId, MatchEventType type, int minute, Guid? teamId = null, Guid? playerId = null, string? notes = null)
    {
        MatchId = matchId;
        Type = type;
        Minute = minute;
        TeamId = teamId;
        PlayerId = playerId;
        Notes = notes;
    }
}
