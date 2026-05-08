using RegionalLeagueApp.Domain.Clubs;
using RegionalLeagueApp.Domain.Collaboration;
using RegionalLeagueApp.Domain.Common;
using RegionalLeagueApp.Domain.Competitions;

namespace RegionalLeagueApp.Domain.Matches;

public sealed class Match : Entity
{
    public Guid CompetitionId { get; private set; }
    public Competition? Competition { get; private set; }
    public Guid RoundId { get; private set; }
    public Round? Round { get; private set; }
    public Guid HomeTeamId { get; private set; }
    public Team? HomeTeam { get; private set; }
    public Guid AwayTeamId { get; private set; }
    public Team? AwayTeam { get; private set; }
    public Guid? VenueId { get; private set; }
    public Venue? Venue { get; private set; }
    public DateTimeOffset StartsAt { get; private set; }
    public MatchStatus Status { get; private set; } = MatchStatus.Scheduled;
    public int? HomeScore { get; private set; }
    public int? AwayScore { get; private set; }
    public List<MatchEvent> Events { get; private set; } = [];
    public List<MatchUpdateProposal> UpdateProposals { get; private set; } = [];

    private Match()
    {
    }

    public Match(
        Guid competitionId,
        Guid roundId,
        Guid homeTeamId,
        Guid awayTeamId,
        DateTimeOffset startsAt,
        Guid? venueId = null,
        MatchStatus status = MatchStatus.Scheduled,
        int? homeScore = null,
        int? awayScore = null)
    {
        if (homeTeamId == awayTeamId)
        {
            throw new ArgumentException("Home and away teams must be different.", nameof(awayTeamId));
        }

        if (homeScore < 0 || awayScore < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(homeScore), "Scores cannot be negative.");
        }

        CompetitionId = competitionId;
        RoundId = roundId;
        HomeTeamId = homeTeamId;
        AwayTeamId = awayTeamId;
        StartsAt = startsAt;
        VenueId = venueId;
        Status = status;
        HomeScore = homeScore;
        AwayScore = awayScore;
    }
}
