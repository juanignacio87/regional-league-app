using RegionalLeagueApp.Domain.Common;
using RegionalLeagueApp.Domain.Competitions;
using RegionalLeagueApp.Domain.Players;

namespace RegionalLeagueApp.Domain.Clubs;

public sealed class Team : Entity
{
    public Guid ClubId { get; private set; }
    public Club? Club { get; private set; }
    public Guid CompetitionId { get; private set; }
    public Competition? Competition { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Category { get; private set; } = "First";
    public bool IsActive { get; private set; } = true;
    public DateTime? ArchivedAt { get; private set; }
    public List<Player> Players { get; private set; } = [];

    private Team()
    {
    }

    public Team(Guid clubId, Guid competitionId, string name, string category = "First")
    {
        ClubId = clubId;
        CompetitionId = competitionId;
        Name = name;
        Category = category;
    }

    public void UpdateDetails(Guid clubId, Guid competitionId, string name, string category)
    {
        ClubId = clubId;
        CompetitionId = competitionId;
        Name = name;
        Category = category;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        ArchivedAt = isActive ? null : DateTime.UtcNow;
    }
}
