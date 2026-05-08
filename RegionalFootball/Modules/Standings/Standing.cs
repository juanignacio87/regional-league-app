using RegionalFootball.Modules.Competitions;
using RegionalFootball.Modules.Clubs;

namespace RegionalFootball.Modules.Standings;

public class Standing
{
    public int Id { get; set; }
    public int CompetitionId { get; set; }
    public Competition? Competition { get; set; }
    public int TeamId { get; set; }
    public Team? Team { get; set; }
    public int Played { get; set; }
    public int Won { get; set; }
    public int Drawn { get; set; }
    public int Lost { get; set; }
    public int GoalsFor { get; set; }
    public int GoalsAgainst { get; set; }
    public int Points { get; set; }
    public int GoalDifference => GoalsFor - GoalsAgainst;
}
