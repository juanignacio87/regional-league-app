namespace RegionalLeagueApp.Application.Collaboration;

public interface IMatchAdministrationPermissionService
{
    Task<MatchAdministrationAccess> GetAccessAsync(CancellationToken cancellationToken = default);
}
