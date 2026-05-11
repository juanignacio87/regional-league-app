namespace RegionalLeagueApp.Application.Collaboration;

public sealed record MatchAdministrationAccess(
    Guid? UserId,
    bool IsAdmin,
    bool IsModerator,
    bool IsContributor,
    IReadOnlyCollection<Guid> AssignedTeamIds)
{
    public static MatchAdministrationAccess Empty { get; } = new(null, false, false, false, []);

    public bool CanAccessAdmin => IsAdmin || IsModerator || IsContributor;
    public bool CanManageAllMatches => IsAdmin || IsModerator;
    public bool CanModerate => IsAdmin || IsModerator;

    public bool CanManageMatch(Guid homeTeamId, Guid awayTeamId) =>
        CanManageAllMatches || AssignedTeamIds.Contains(homeTeamId) || AssignedTeamIds.Contains(awayTeamId);

    public string RoleLabel => IsAdmin
        ? "Admin"
        : IsModerator
            ? "Moderator"
            : IsContributor
                ? "Contributor"
                : "Sin permisos";
}
