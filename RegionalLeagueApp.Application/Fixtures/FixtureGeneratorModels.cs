namespace RegionalLeagueApp.Application.Fixtures;

public interface IFixtureGeneratorService
{
    FixtureGeneratorPreviewResult Preview(FixtureGeneratorRequest request);
}

public sealed record FixtureGeneratorRequest(
    IReadOnlyList<FixtureGeneratorTeam> Teams,
    DateOnly StartDate,
    int DaysBetweenRounds,
    TimeOnly DefaultTime,
    bool HomeAndAway);

public sealed record FixtureGeneratorTeam(Guid Id, string Name);

public sealed record FixtureGeneratorPreviewResult(IReadOnlyList<FixtureGeneratorRound> Rounds, IReadOnlyList<string> ValidationErrors)
{
    public int RoundCount => Rounds.Count;

    public int MatchCount => Rounds.Sum(round => round.Matches.Count(match => !match.IsBye));

    public bool IsValid => ValidationErrors.Count == 0;
}

public sealed record FixtureGeneratorRound(
    int RoundNumber,
    string RoundName,
    DateOnly Date,
    IReadOnlyList<FixtureGeneratorMatch> Matches);

public sealed record FixtureGeneratorMatch(
    int RoundNumber,
    string RoundName,
    DateOnly Date,
    TimeOnly Time,
    Guid HomeTeamId,
    string HomeTeamName,
    Guid? AwayTeamId,
    string? AwayTeamName,
    bool IsBye);

public sealed class FixtureGeneratorService : IFixtureGeneratorService
{
    public FixtureGeneratorPreviewResult Preview(FixtureGeneratorRequest request)
    {
        if (request.Teams.Count < 2)
        {
            return new FixtureGeneratorPreviewResult([], ["La competencia necesita al menos dos equipos."]);
        }

        var teams = request.Teams
            .Select(team => new FixtureSlot(team.Id, team.Name, false))
            .ToList();

        if (teams.Count % 2 != 0)
        {
            teams.Add(new FixtureSlot(Guid.Empty, "Libre", true));
        }

        var firstLeg = BuildLeg(teams, request.StartDate, request.DaysBetweenRounds, request.DefaultTime, 1, reverseHomeAway: false);
        if (!request.HomeAndAway)
        {
            return new FixtureGeneratorPreviewResult(firstLeg, ValidateRounds(firstLeg));
        }

        var secondLegStart = firstLeg.Count + 1;
        var secondLeg = BuildLeg(teams, request.StartDate, request.DaysBetweenRounds, request.DefaultTime, secondLegStart, reverseHomeAway: true);

        var rounds = firstLeg.Concat(secondLeg).ToList();
        return new FixtureGeneratorPreviewResult(rounds, ValidateRounds(rounds));
    }

    private static List<FixtureGeneratorRound> BuildLeg(
        IReadOnlyList<FixtureSlot> sourceTeams,
        DateOnly startDate,
        int daysBetweenRounds,
        TimeOnly defaultTime,
        int roundNumberOffset,
        bool reverseHomeAway)
    {
        var rotation = sourceTeams.ToList();
        var roundsPerLeg = rotation.Count - 1;
        var matchesPerRound = rotation.Count / 2;
        var rounds = new List<FixtureGeneratorRound>();

        for (var roundIndex = 0; roundIndex < roundsPerLeg; roundIndex++)
        {
            var roundNumber = roundNumberOffset + roundIndex;
            var roundName = $"Fecha {roundNumber}";
            var date = startDate.AddDays((roundNumber - 1) * daysBetweenRounds);
            var matches = new List<FixtureGeneratorMatch>();

            for (var pairIndex = 0; pairIndex < matchesPerRound; pairIndex++)
            {
                var left = rotation[pairIndex];
                var right = rotation[rotation.Count - 1 - pairIndex];
                var swap = roundIndex % 2 == 1;
                var home = swap ? right : left;
                var away = swap ? left : right;

                if (reverseHomeAway)
                {
                    (home, away) = (away, home);
                }

                if (home.IsBye || away.IsBye)
                {
                    var freeTeam = home.IsBye ? away : home;
                    matches.Add(new FixtureGeneratorMatch(roundNumber, roundName, date, defaultTime, freeTeam.Id, freeTeam.Name, null, null, true));
                    continue;
                }

                matches.Add(new FixtureGeneratorMatch(roundNumber, roundName, date, defaultTime, home.Id, home.Name, away.Id, away.Name, false));
            }

            rounds.Add(new FixtureGeneratorRound(roundNumber, roundName, date, matches));
            rotation = Rotate(rotation);
        }

        return rounds;
    }

    private static List<FixtureSlot> Rotate(IReadOnlyList<FixtureSlot> teams)
    {
        var rotated = new List<FixtureSlot> { teams[0], teams[^1] };
        rotated.AddRange(teams.Skip(1).Take(teams.Count - 2));
        return rotated;
    }

    private static List<string> ValidateRounds(IReadOnlyList<FixtureGeneratorRound> rounds)
    {
        var errors = new List<string>();
        foreach (var round in rounds)
        {
            var repeatedTeams = round.Matches
                .SelectMany(match => match.IsBye
                    ? [match.HomeTeamId]
                    : new[] { match.HomeTeamId, match.AwayTeamId!.Value })
                .GroupBy(teamId => teamId)
                .Where(group => group.Count() > 1)
                .ToList();

            foreach (var repeatedTeam in repeatedTeams)
            {
                var teamName = round.Matches
                    .SelectMany(match => new[]
                    {
                        (TeamId: match.HomeTeamId, Name: match.HomeTeamName),
                        (TeamId: match.AwayTeamId, Name: match.AwayTeamName ?? string.Empty)
                    })
                    .FirstOrDefault(team => team.TeamId == repeatedTeam.Key).Name;
                errors.Add($"{round.RoundName}: {teamName} aparece mas de una vez.");
            }
        }

        return errors;
    }

    private sealed record FixtureSlot(Guid Id, string Name, bool IsBye);
}
