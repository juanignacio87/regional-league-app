namespace RegionalLeagueApp.Application.Players;

public interface IPlayerCsvImportService
{
    Task<PlayerCsvPreviewResult> PreviewAsync(Guid clubId, string csv, CancellationToken cancellationToken = default);

    Task<PlayerCsvImportResult> ImportAsync(Guid clubId, IReadOnlyCollection<int> validRowNumbers, string csv, CancellationToken cancellationToken = default);
}

public sealed record PlayerCsvPreviewResult(IReadOnlyList<PlayerCsvPreviewRow> Rows)
{
    public int ValidCount => Rows.Count(x => x.Status == PlayerCsvRowStatus.Valid);
}

public sealed record PlayerCsvImportResult(int ImportedCount, PlayerCsvPreviewResult Preview);

public sealed record PlayerCsvPreviewRow(
    int RowNumber,
    string Team,
    string DisplayName,
    string ShirtNumber,
    string Position,
    PlayerCsvRowStatus Status,
    string Message,
    Guid? TeamId = null);

public enum PlayerCsvRowStatus
{
    Valid,
    Error,
    Duplicate
}
