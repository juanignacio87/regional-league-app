using System.Globalization;
using Microsoft.EntityFrameworkCore;
using RegionalLeagueApp.Application.Fixtures;
using RegionalLeagueApp.Domain.Competitions;
using RegionalLeagueApp.Domain.Matches;
using RegionalLeagueApp.Infrastructure.Persistence;

namespace RegionalLeagueApp.Infrastructure.Fixtures;

public sealed class EfFixtureCsvImportService(ApplicationDbContext db, IFixtureRulesService fixtureRules) : IFixtureCsvImportService
{
    public async Task<FixtureCsvPreviewResult> PreviewAsync(Guid competitionId, string csv, CancellationToken cancellationToken = default)
    {
        var teams = await db.Teams
            .AsNoTracking()
            .Where(x => x.CompetitionId == competitionId && x.IsActive && x.Club != null && x.Club.IsActive)
            .Select(x => new TeamLookupItem(x.Id, x.Name, x.Club!.Name, x.Club.ShortName))
            .ToListAsync(cancellationToken);

        var rows = ParseRows(csv);
        var preview = new List<FixtureCsvPreviewRow>();
        var csvExactKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var csvRoundTeamKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            preview.Add(await ValidateRowAsync(competitionId, row, teams, csvExactKeys, csvRoundTeamKeys, cancellationToken));
        }

        return new FixtureCsvPreviewResult(preview);
    }

    public async Task<FixtureCsvImportResult> ImportAsync(Guid competitionId, IReadOnlyCollection<int> validRowNumbers, string csv, CancellationToken cancellationToken = default)
    {
        var preview = await PreviewAsync(competitionId, csv, cancellationToken);
        var rowsToImport = preview.Rows
            .Where(x => validRowNumbers.Contains(x.RowNumber) && x.Status == FixtureCsvRowStatus.Valid && x.HomeTeamId is not null && x.AwayTeamId is not null && x.StartsAtUtc is not null)
            .ToList();

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var roundCache = new Dictionary<int, Round>();

        try
        {
            foreach (var row in rowsToImport)
            {
                var sortOrder = int.Parse(row.Round, CultureInfo.InvariantCulture);
                var round = await FindOrCreateRoundAsync(competitionId, sortOrder, roundCache, cancellationToken);
                db.Matches.Add(new Match(competitionId, round.Id, row.HomeTeamId!.Value, row.AwayTeamId!.Value, row.StartsAtUtc!.Value, status: MatchStatus.Scheduled));
            }

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        return new FixtureCsvImportResult(rowsToImport.Count, preview);
    }

    private static List<CsvFixtureRow> ParseRows(string csv)
    {
        var result = new List<CsvFixtureRow>();
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
            if (parts.Length >= 5 && parts[0].Equals("Round", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            dataRowNumber++;
            result.Add(new CsvFixtureRow(
                dataRowNumber,
                parts.ElementAtOrDefault(0) ?? string.Empty,
                parts.ElementAtOrDefault(1) ?? string.Empty,
                parts.ElementAtOrDefault(2) ?? string.Empty,
                parts.ElementAtOrDefault(3) ?? string.Empty,
                parts.ElementAtOrDefault(4) ?? string.Empty,
                parts.Length));
        }

        return result;
    }

    private async Task<FixtureCsvPreviewRow> ValidateRowAsync(
        Guid competitionId,
        CsvFixtureRow row,
        IReadOnlyCollection<TeamLookupItem> teams,
        HashSet<string> csvExactKeys,
        HashSet<string> csvRoundTeamKeys,
        CancellationToken cancellationToken)
    {
        var startsAtText = $"{row.Date} {row.Time}";

        if (row.ColumnCount != 5)
        {
            return Error(row, startsAtText, "La fila debe tener exactamente 5 columnas.");
        }

        if (!int.TryParse(row.Round, out var roundNumber) || roundNumber <= 0)
        {
            return Error(row, startsAtText, "Round debe ser un entero positivo.");
        }

        if (!DateOnly.TryParseExact(row.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return Error(row, startsAtText, "Date debe usar formato yyyy-MM-dd.");
        }

        if (!TimeOnly.TryParseExact(row.Time, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var time))
        {
            return Error(row, startsAtText, "Time debe usar formato HH:mm.");
        }

        if (string.IsNullOrWhiteSpace(row.HomeTeam) || string.IsNullOrWhiteSpace(row.AwayTeam))
        {
            return Error(row, startsAtText, "HomeTeam y AwayTeam son obligatorios.");
        }

        if (row.HomeTeam.Equals(row.AwayTeam, StringComparison.OrdinalIgnoreCase))
        {
            return Error(row, startsAtText, "Local y visitante deben ser distintos.");
        }

        var homeMatch = MatchTeam(row.HomeTeam, teams);
        if (homeMatch.Error is not null)
        {
            return Error(row, startsAtText, $"Local: {homeMatch.Error}");
        }

        var awayMatch = MatchTeam(row.AwayTeam, teams);
        if (awayMatch.Error is not null)
        {
            return Error(row, startsAtText, $"Visitante: {awayMatch.Error}");
        }

        if (homeMatch.Team!.Id == awayMatch.Team!.Id)
        {
            return Error(row, startsAtText, "Local y visitante resuelven al mismo equipo.");
        }

        var startsAt = new DateTimeOffset(DateTime.SpecifyKind(date.ToDateTime(time), DateTimeKind.Local)).ToUniversalTime();
        var roundSortOrder = roundNumber;
        var ruleValidation = await fixtureRules.ValidateMatchCreationAsync(
            new FixtureMatchRulesRequest(
                competitionId,
                RoundName(roundSortOrder),
                roundSortOrder,
                homeMatch.Team.Id,
                awayMatch.Team.Id,
                startsAt),
            cancellationToken);

        if (!ruleValidation.IsValid)
        {
            return Duplicate(row, startsAtText, ruleValidation.FirstError ?? "La fila no cumple las reglas de fixture.", homeMatch.Team.Id, awayMatch.Team.Id, startsAt);
        }

        var exactKey = ExactKey(homeMatch.Team.Id, awayMatch.Team.Id, startsAt);
        if (!csvExactKeys.Add(exactKey))
        {
            return Duplicate(row, startsAtText, "Duplicado exacto dentro del CSV.", homeMatch.Team.Id, awayMatch.Team.Id, startsAt);
        }

        var homeRoundKey = RoundTeamKey(roundSortOrder, homeMatch.Team.Id);
        var awayRoundKey = RoundTeamKey(roundSortOrder, awayMatch.Team.Id);
        if (!csvRoundTeamKeys.Add(homeRoundKey) || !csvRoundTeamKeys.Add(awayRoundKey))
        {
            return Duplicate(row, startsAtText, "Un equipo aparece mas de una vez en la misma jornada dentro del CSV.", homeMatch.Team.Id, awayMatch.Team.Id, startsAt);
        }

        return new FixtureCsvPreviewRow(row.RowNumber, row.Round, startsAtText, homeMatch.Team.Label, awayMatch.Team.Label, FixtureCsvRowStatus.Valid, "Lista para importar.", homeMatch.Team.Id, awayMatch.Team.Id, startsAt);
    }

    private async Task<Round> FindOrCreateRoundAsync(Guid competitionId, int sortOrder, Dictionary<int, Round> roundCache, CancellationToken cancellationToken)
    {
        if (roundCache.TryGetValue(sortOrder, out var cachedRound))
        {
            return cachedRound;
        }

        var localRound = db.Rounds.Local.FirstOrDefault(x => x.CompetitionId == competitionId && x.SortOrder == sortOrder);
        if (localRound is not null)
        {
            roundCache[sortOrder] = localRound;
            return localRound;
        }

        var round = await db.Rounds.FirstOrDefaultAsync(x => x.CompetitionId == competitionId && x.SortOrder == sortOrder, cancellationToken);
        if (round is not null)
        {
            roundCache[sortOrder] = round;
            return round;
        }

        round = new Round(competitionId, RoundName(sortOrder), sortOrder);
        db.Rounds.Add(round);
        roundCache[sortOrder] = round;
        return round;
    }

    private static TeamMatchResult MatchTeam(string input, IReadOnlyCollection<TeamLookupItem> teams)
    {
        var matches = teams
            .Where(x =>
                x.TeamName.Equals(input, StringComparison.OrdinalIgnoreCase) ||
                x.ClubName.Equals(input, StringComparison.OrdinalIgnoreCase) ||
                x.ClubShortName.Equals(input, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return matches.Count switch
        {
            0 => new TeamMatchResult(null, "equipo no encontrado."),
            1 => new TeamMatchResult(matches[0], null),
            _ => new TeamMatchResult(null, "nombre ambiguo.")
        };
    }

    private static string RoundName(int sortOrder) => $"Fecha {sortOrder}";
    private static string ExactKey(Guid homeTeamId, Guid awayTeamId, DateTimeOffset startsAt) => $"{homeTeamId:N}|{awayTeamId:N}|{startsAt:O}";
    private static string RoundTeamKey(int sortOrder, Guid teamId) => $"{sortOrder}|{teamId:N}";

    private static FixtureCsvPreviewRow Error(CsvFixtureRow row, string startsAt, string message) =>
        new(row.RowNumber, row.Round, startsAt, row.HomeTeam, row.AwayTeam, FixtureCsvRowStatus.Error, message);

    private static FixtureCsvPreviewRow Duplicate(CsvFixtureRow row, string startsAt, string message, Guid homeTeamId, Guid awayTeamId, DateTimeOffset startsAtUtc) =>
        new(row.RowNumber, row.Round, startsAt, row.HomeTeam, row.AwayTeam, FixtureCsvRowStatus.Duplicate, message, homeTeamId, awayTeamId, startsAtUtc);

    private sealed record CsvFixtureRow(int RowNumber, string Round, string Date, string Time, string HomeTeam, string AwayTeam, int ColumnCount);
    private sealed record TeamLookupItem(Guid Id, string TeamName, string ClubName, string ClubShortName)
    {
        public string Label => $"{ClubName} - {TeamName}";
    }
    private sealed record TeamMatchResult(TeamLookupItem? Team, string? Error);
}
