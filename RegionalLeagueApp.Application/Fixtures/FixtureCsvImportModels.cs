namespace RegionalLeagueApp.Application.Fixtures;

public interface IFixtureCsvImportService
{
    Task<FixtureCsvPreviewResult> PreviewAsync(Guid competitionId, string csv, CancellationToken cancellationToken = default);

    Task<FixtureCsvImportResult> ImportAsync(Guid competitionId, IReadOnlyCollection<int> validRowNumbers, string csv, CancellationToken cancellationToken = default);
}

public sealed record FixtureCsvPreviewResult(IReadOnlyList<FixtureCsvPreviewRow> Rows)
{
    public int ValidCount => Rows.Count(x => x.Status == FixtureCsvRowStatus.Valid);
}

public sealed record FixtureCsvImportResult(int ImportedCount, FixtureCsvPreviewResult Preview);

public sealed record FixtureCsvPreviewRow(
    int RowNumber,
    string Round,
    string StartsAt,
    string HomeTeam,
    string AwayTeam,
    FixtureCsvRowStatus Status,
    string Message,
    Guid? HomeTeamId = null,
    Guid? AwayTeamId = null,
    DateTimeOffset? StartsAtUtc = null);

public enum FixtureCsvRowStatus
{
    Valid,
    Error,
    Duplicate
}
