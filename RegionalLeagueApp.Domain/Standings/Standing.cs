using RegionalLeagueApp.Domain.Clubs;
using RegionalLeagueApp.Domain.Common;
using RegionalLeagueApp.Domain.Competitions;

namespace RegionalLeagueApp.Domain.Standings;

public sealed class Standing : Entity
{
    public Guid CompetitionId { get; private set; }
    public Competition? Competition { get; private set; }
    public Guid TeamId { get; private set; }
    public Team? Team { get; private set; }
    public int Played { get; private set; }
    public int Won { get; private set; }
    public int Drawn { get; private set; }
    public int Lost { get; private set; }
    public int GoalsFor { get; private set; }
    public int GoalsAgainst { get; private set; }
    public int Points { get; private set; }
    public int GoalDifference => GoalsFor - GoalsAgainst;

    private Standing()
    {
    }

    public Standing(
        Guid competitionId,
        Guid teamId,
        int played = 0,
        int won = 0,
        int drawn = 0,
        int lost = 0,
        int goalsFor = 0,
        int goalsAgainst = 0,
        int points = 0)
    {
        CompetitionId = competitionId;
        TeamId = teamId;
        Played = played;
        Won = won;
        Drawn = drawn;
        Lost = lost;
        GoalsFor = goalsFor;
        GoalsAgainst = goalsAgainst;
        Points = points;
    }
}
