using Microsoft.AspNetCore.SignalR;

namespace RegionalLeagueApp.Web.Hubs;

public sealed class MatchUpdatesHub : Hub
{
    public const string Route = "/hubs/match-updates";
    public const string MatchDataChanged = "MatchDataChanged";
}
