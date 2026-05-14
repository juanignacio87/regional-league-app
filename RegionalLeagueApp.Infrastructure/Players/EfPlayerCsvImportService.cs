using System.Globalization;
using Microsoft.EntityFrameworkCore;
using RegionalLeagueApp.Application.Players;
using RegionalLeagueApp.Domain.Players;
using RegionalLeagueApp.Infrastructure.Persistence;

namespace RegionalLeagueApp.Infrastructure.Players;

public sealed class EfPlayerCsvImportService(ApplicationDbContext db) : IPlayerCsvImportService
{
    public async Task<PlayerCsvPreviewResult> PreviewAsync(Guid clubId, string csv, CancellationToken cancellationToken = default)
    {
        var teams = await db.Teams
            .AsNoTracking()
            .Where(x => x.ClubId == clubId && x.IsActive && x.Club != null && x.Club.IsActive)
            .Select(x => new TeamLookupItem(x.Id, x.Name, x.Category, x.Club!.ShortName))
            .ToListAsync(cancellationToken);

        var activePlayers = await db.Players
            .AsNoTracking()
            .Where(x => x.IsActive && x.Team != null && x.Team.ClubId == clubId)
            .Select(x => new ExistingPlayerItem(x.TeamId, x.DisplayName, x.ShirtNumber))
            .ToListAsync(cancellationToken);

        var rows = ParseRows(csv);
        var preview = new List<PlayerCsvPreviewRow>();
        var csvDisplayKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var csvShirtKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            preview.Add(ValidateRow(row, teams, activePlayers, csvDisplayKeys, csvShirtKeys));
        }

        return new PlayerCsvPreviewResult(preview);
    }

    public async Task<PlayerCsvImportResult> ImportAsync(Guid clubId, IReadOnlyCollection<int> validRowNumbers, string csv, CancellationToken cancellationToken = default)
    {
        var preview = await PreviewAsync(clubId, csv, cancellationToken);
        var rowsByNumber = ParseRows(csv).ToDictionary(x => x.RowNumber);
        var rowsToImport = preview.Rows
            .Where(x => validRowNumbers.Contains(x.RowNumber) && x.Status == PlayerCsvRowStatus.Valid && x.TeamId is not null)
            .ToList();

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            foreach (var previewRow in rowsToImport)
            {
                var row = rowsByNumber[previewRow.RowNumber];
                var player = new Player(
                    previewRow.TeamId!.Value,
                    row.FirstName.Trim(),
                    row.LastName.Trim(),
                    ParsePosition(row.Position),
                    ParseShirtNumber(row.ShirtNumber),
                    ParseBirthDate(row.BirthDate));

                db.Players.Add(player);
                db.Entry(player).Property(x => x.DisplayName).CurrentValue = row.DisplayName.Trim();
            }

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        return new PlayerCsvImportResult(rowsToImport.Count, preview);
    }

    private static List<CsvPlayerRow> ParseRows(string csv)
    {
        var result = new List<CsvPlayerRow>();
        var lines = csv.Split(["\r\n", "\n"], StringSplitOptions.None);
        var dataRowNumber = 0;

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var parts = line.Split(',').Select(x => x.Trim()).ToArray();
            if (parts.Length >= 7 && parts[0].Equals("Team", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            dataRowNumber++;
            result.Add(new CsvPlayerRow(
                dataRowNumber,
                parts.ElementAtOrDefault(0) ?? string.Empty,
                parts.ElementAtOrDefault(1) ?? string.Empty,
                parts.ElementAtOrDefault(2) ?? string.Empty,
                parts.ElementAtOrDefault(3) ?? string.Empty,
                parts.ElementAtOrDefault(4) ?? string.Empty,
                parts.ElementAtOrDefault(5) ?? string.Empty,
                parts.ElementAtOrDefault(6) ?? string.Empty,
                parts.Length));
        }

        return result;
    }

    private static PlayerCsvPreviewRow ValidateRow(
        CsvPlayerRow row,
        IReadOnlyCollection<TeamLookupItem> teams,
        IReadOnlyCollection<ExistingPlayerItem> activePlayers,
        HashSet<string> csvDisplayKeys,
        HashSet<string> csvShirtKeys)
    {
        if (row.ColumnCount != 7)
        {
            return Error(row, "La fila debe tener exactamente 7 columnas.");
        }

        var teamMatch = MatchTeam(row.Team, teams);
        if (teamMatch.Error is not null)
        {
            return Error(row, $"Team: {teamMatch.Error}");
        }

        if (string.IsNullOrWhiteSpace(row.DisplayName))
        {
            return Error(row, "DisplayName es obligatorio.");
        }

        var shirtNumber = ParseShirtNumber(row.ShirtNumber);
        if (!string.IsNullOrWhiteSpace(row.ShirtNumber) && shirtNumber is null)
        {
            return Error(row, "ShirtNumber debe ser un entero positivo.");
        }

        var birthDate = ParseBirthDate(row.BirthDate);
        if (!string.IsNullOrWhiteSpace(row.BirthDate) && birthDate is null)
        {
            return Error(row, "BirthDate debe usar formato yyyy-MM-dd.");
        }

        if (birthDate is not null && birthDate > DateOnly.FromDateTime(DateTime.Today))
        {
            return Error(row, "BirthDate no puede estar en el futuro.");
        }

        if (!string.IsNullOrWhiteSpace(row.Position) && !Enum.TryParse<PlayerPosition>(row.Position, true, out _))
        {
            return Error(row, "Position no es valida.");
        }

        var team = teamMatch.Team!;
        var displayKey = DisplayKey(team.Id, row.DisplayName);
        if (activePlayers.Any(x => x.TeamId == team.Id && Normalize(x.DisplayName) == Normalize(row.DisplayName)))
        {
            return Duplicate(row, "Ya existe un jugador activo con ese display name en el equipo.", team.Id);
        }

        if (!csvDisplayKeys.Add(displayKey))
        {
            return Duplicate(row, "DisplayName duplicado dentro del CSV para el mismo equipo.", team.Id);
        }

        if (shirtNumber is not null)
        {
            if (activePlayers.Any(x => x.TeamId == team.Id && x.ShirtNumber == shirtNumber))
            {
                return Duplicate(row, "Ya existe un jugador activo con ese dorsal en el equipo.", team.Id);
            }

            if (!csvShirtKeys.Add(ShirtKey(team.Id, shirtNumber.Value)))
            {
                return Duplicate(row, "Dorsal duplicado dentro del CSV para el mismo equipo.", team.Id);
            }
        }

        return new PlayerCsvPreviewRow(row.RowNumber, team.Label, row.DisplayName, row.ShirtNumber, string.IsNullOrWhiteSpace(row.Position) ? "Unknown" : row.Position, PlayerCsvRowStatus.Valid, "Lista para importar.", team.Id);
    }

    private static TeamMatchResult MatchTeam(string input, IReadOnlyCollection<TeamLookupItem> teams)
    {
        var normalizedInput = Normalize(input);
        var matches = teams
            .Where(x =>
                Normalize(x.TeamName) == normalizedInput ||
                Normalize($"{x.ClubShortName} {x.Category}") == normalizedInput)
            .ToList();

        return matches.Count switch
        {
            0 => new TeamMatchResult(null, "equipo no encontrado."),
            1 => new TeamMatchResult(matches[0], null),
            _ => new TeamMatchResult(null, "nombre ambiguo.")
        };
    }

    private static int? ParseShirtNumber(string value) =>
        int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var parsed) && parsed > 0 ? parsed : null;

    private static DateOnly? ParseBirthDate(string value) =>
        DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed) ? parsed : null;

    private static PlayerPosition ParsePosition(string value) =>
        Enum.TryParse<PlayerPosition>(value, true, out var parsed) ? parsed : PlayerPosition.Unknown;

    private static string Normalize(string value) =>
        string.Join(' ', value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)).ToUpperInvariant();

    private static string DisplayKey(Guid teamId, string displayName) => $"{teamId:N}|{Normalize(displayName)}";
    private static string ShirtKey(Guid teamId, int shirtNumber) => $"{teamId:N}|{shirtNumber}";

    private static PlayerCsvPreviewRow Error(CsvPlayerRow row, string message) =>
        new(row.RowNumber, row.Team, row.DisplayName, row.ShirtNumber, row.Position, PlayerCsvRowStatus.Error, message);

    private static PlayerCsvPreviewRow Duplicate(CsvPlayerRow row, string message, Guid teamId) =>
        new(row.RowNumber, row.Team, row.DisplayName, row.ShirtNumber, row.Position, PlayerCsvRowStatus.Duplicate, message, teamId);

    private sealed record CsvPlayerRow(int RowNumber, string Team, string DisplayName, string FirstName, string LastName, string ShirtNumber, string Position, string BirthDate, int ColumnCount);
    private sealed record TeamLookupItem(Guid Id, string TeamName, string Category, string ClubShortName)
    {
        public string Label => TeamName;
    }
    private sealed record ExistingPlayerItem(Guid TeamId, string DisplayName, int? ShirtNumber);
    private sealed record TeamMatchResult(TeamLookupItem? Team, string? Error);
}
