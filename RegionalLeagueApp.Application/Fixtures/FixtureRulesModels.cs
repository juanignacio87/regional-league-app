namespace RegionalLeagueApp.Application.Fixtures;

public interface IFixtureRulesService
{
    Task<FixtureRuleValidationResult> ValidateMatchCreationAsync(
        FixtureMatchRulesRequest request,
        CancellationToken cancellationToken = default);

    Task<FixtureRuleValidationResult> ValidateMatchUpdateAsync(
        Guid currentMatchId,
        FixtureMatchRulesRequest request,
        CancellationToken cancellationToken = default);

    Task<FixtureRuleValidationResult> ValidateFixtureBatchAsync(
        Guid competitionId,
        IReadOnlyCollection<FixtureBatchMatchRequest> matches,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Guid>> GetSafeDeletableMatchIdsAsync(
        Guid competitionId,
        CancellationToken cancellationToken = default);

    Task<bool> HasBlockingMatchesForRegenerationAsync(
        Guid competitionId,
        CancellationToken cancellationToken = default);
}

public sealed record FixtureMatchRulesRequest(
    Guid CompetitionId,
    string RoundName,
    int? RoundSortOrder,
    Guid HomeTeamId,
    Guid AwayTeamId,
    DateTimeOffset StartsAt);

public sealed record FixtureBatchMatchRequest(
    string Label,
    string RoundName,
    int? RoundSortOrder,
    Guid HomeTeamId,
    Guid AwayTeamId,
    DateTimeOffset StartsAt);

public sealed record FixtureRuleValidationResult(IReadOnlyList<string> Errors)
{
    public bool IsValid => Errors.Count == 0;

    public string? FirstError => Errors.FirstOrDefault();

    public static FixtureRuleValidationResult Success { get; } = new([]);

    public static FixtureRuleValidationResult Failure(params string[] errors) => new(errors);
}
