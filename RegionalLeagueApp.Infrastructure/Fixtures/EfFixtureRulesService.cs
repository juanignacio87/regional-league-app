using Microsoft.EntityFrameworkCore;
using RegionalLeagueApp.Application.Fixtures;
using RegionalLeagueApp.Domain.Matches;
using RegionalLeagueApp.Infrastructure.Persistence;

namespace RegionalLeagueApp.Infrastructure.Fixtures;

public sealed class EfFixtureRulesService(ApplicationDbContext db) : IFixtureRulesService
{
    public Task<FixtureRuleValidationResult> ValidateMatchCreationAsync(
        FixtureMatchRulesRequest request,
        CancellationToken cancellationToken = default) =>
        ValidateSingleMatchAsync(request, null, cancellationToken);

    public Task<FixtureRuleValidationResult> ValidateMatchUpdateAsync(
        Guid currentMatchId,
        FixtureMatchRulesRequest request,
        CancellationToken cancellationToken = default) =>
        ValidateSingleMatchAsync(request, currentMatchId, cancellationToken);

    public async Task<FixtureRuleValidationResult> ValidateFixtureBatchAsync(
        Guid competitionId,
        IReadOnlyCollection<FixtureBatchMatchRequest> matches,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();
        var batchExactKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var batchRoundTeamKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var match in matches)
        {
            var validation = await ValidateSingleMatchAsync(
                new FixtureMatchRulesRequest(
                    competitionId,
                    match.RoundName,
                    match.RoundSortOrder,
                    match.HomeTeamId,
                    match.AwayTeamId,
                    match.StartsAt),
                null,
                cancellationToken);

            if (!validation.IsValid)
            {
                errors.AddRange(validation.Errors.Select(error => $"{match.Label}: {error}"));
                continue;
            }

            var exactKey = ExactKey(match.HomeTeamId, match.AwayTeamId, match.StartsAt);
            if (!batchExactKeys.Add(exactKey))
            {
                errors.Add($"{match.Label}: duplicado exacto dentro del lote.");
            }

            var roundKey = RoundKey(match.RoundName, match.RoundSortOrder);
            if (!batchRoundTeamKeys.Add(RoundTeamKey(roundKey, match.HomeTeamId)) ||
                !batchRoundTeamKeys.Add(RoundTeamKey(roundKey, match.AwayTeamId)))
            {
                errors.Add($"{match.Label}: un equipo aparece mas de una vez en la misma jornada dentro del lote.");
            }
        }

        return errors.Count == 0
            ? FixtureRuleValidationResult.Success
            : new FixtureRuleValidationResult(errors);
    }

    public async Task<IReadOnlyList<Guid>> GetSafeDeletableMatchIdsAsync(
        Guid competitionId,
        CancellationToken cancellationToken = default)
    {
        return await SafeDeletableMatchesQuery(competitionId)
            .Select(match => match.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> HasBlockingMatchesForRegenerationAsync(
        Guid competitionId,
        CancellationToken cancellationToken = default)
    {
        return db.Matches.AnyAsync(match =>
            match.CompetitionId == competitionId &&
            !(match.Status == MatchStatus.Scheduled &&
              match.HomeScore == null &&
              match.AwayScore == null &&
              !match.Events.Any()),
            cancellationToken);
    }

    private async Task<FixtureRuleValidationResult> ValidateSingleMatchAsync(
        FixtureMatchRulesRequest request,
        Guid? currentMatchId,
        CancellationToken cancellationToken)
    {
        if (request.HomeTeamId == request.AwayTeamId)
        {
            return FixtureRuleValidationResult.Failure("Local y visitante deben ser distintos.");
        }

        var activeTeamIds = await db.Teams
            .AsNoTracking()
            .Where(team =>
                team.CompetitionId == request.CompetitionId &&
                team.IsActive &&
                team.Club != null &&
                team.Club.IsActive)
            .Select(team => team.Id)
            .ToListAsync(cancellationToken);

        if (!activeTeamIds.Contains(request.HomeTeamId) || !activeTeamIds.Contains(request.AwayTeamId))
        {
            return FixtureRuleValidationResult.Failure("Ambos equipos deben estar activos y pertenecer a la competencia.");
        }

        if (await TeamAlreadyPlaysInRoundAsync(request, currentMatchId, cancellationToken))
        {
            return FixtureRuleValidationResult.Failure("Un equipo no puede jugar mas de un partido en la misma jornada de la competencia.");
        }

        var duplicateMatch = await db.Matches.AnyAsync(match =>
            match.CompetitionId == request.CompetitionId &&
            (currentMatchId == null || match.Id != currentMatchId.Value) &&
            match.HomeTeamId == request.HomeTeamId &&
            match.AwayTeamId == request.AwayTeamId &&
            match.StartsAt == request.StartsAt,
            cancellationToken);

        return duplicateMatch
            ? FixtureRuleValidationResult.Failure("Ya existe un partido con el mismo local, visitante y fecha/hora.")
            : FixtureRuleValidationResult.Success;
    }

    private Task<bool> TeamAlreadyPlaysInRoundAsync(
        FixtureMatchRulesRequest request,
        Guid? currentMatchId,
        CancellationToken cancellationToken)
    {
        var roundName = request.RoundName.Trim();
        return db.Matches.AnyAsync(match =>
            match.CompetitionId == request.CompetitionId &&
            (currentMatchId == null || match.Id != currentMatchId.Value) &&
            ((request.RoundSortOrder != null && match.Round != null && match.Round.SortOrder == request.RoundSortOrder) ||
             (match.Round != null && match.Round.Name == roundName)) &&
            (match.HomeTeamId == request.HomeTeamId ||
             match.AwayTeamId == request.HomeTeamId ||
             match.HomeTeamId == request.AwayTeamId ||
             match.AwayTeamId == request.AwayTeamId),
            cancellationToken);
    }

    private IQueryable<Match> SafeDeletableMatchesQuery(Guid competitionId) =>
        db.Matches.Where(match =>
            match.CompetitionId == competitionId &&
            match.Status == MatchStatus.Scheduled &&
            match.HomeScore == null &&
            match.AwayScore == null &&
            !match.Events.Any());

    private static string ExactKey(Guid homeTeamId, Guid awayTeamId, DateTimeOffset startsAt) =>
        $"{homeTeamId:N}|{awayTeamId:N}|{startsAt:O}";

    private static string RoundKey(string roundName, int? sortOrder) =>
        sortOrder?.ToString() ?? roundName.Trim().ToUpperInvariant();

    private static string RoundTeamKey(string roundKey, Guid teamId) => $"{roundKey}|{teamId:N}";
}
