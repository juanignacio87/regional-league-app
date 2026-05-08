using RegionalLeagueApp.Domain.Common;
using RegionalLeagueApp.Domain.Matches;

namespace RegionalLeagueApp.Domain.Competitions;

public sealed class Round : Entity
{
    public Guid CompetitionId { get; private set; }
    public Competition? Competition { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int SortOrder { get; private set; }
    public List<Match> Matches { get; private set; } = [];

    private Round()
    {
    }

    public Round(Guid competitionId, string name, int sortOrder)
    {
        CompetitionId = competitionId;
        Name = name;
        SortOrder = sortOrder;
    }
}
