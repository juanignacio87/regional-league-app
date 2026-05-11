using Microsoft.EntityFrameworkCore;
using RegionalLeagueApp.Application.Abstractions.Identity;
using RegionalLeagueApp.Application.Collaboration;
using RegionalLeagueApp.Infrastructure.Identity;
using RegionalLeagueApp.Infrastructure.Persistence;

namespace RegionalLeagueApp.Infrastructure.Collaboration;

public sealed class EfMatchAdministrationPermissionService(
    ApplicationDbContext db,
    ICurrentUser currentUser) : IMatchAdministrationPermissionService
{
    public async Task<MatchAdministrationAccess> GetAccessAsync(CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is not { } userId)
        {
            return MatchAdministrationAccess.Empty;
        }

        var isAdmin = currentUser.IsInRole(AppRoles.Admin);
        var isModerator = currentUser.IsInRole(AppRoles.Moderator);
        var isContributor = currentUser.IsInRole(AppRoles.Contributor);

        var assignedTeamIds = isContributor && !isAdmin && !isModerator
            ? await db.ClubContributors
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.CanReportMatches && x.TeamId != null)
                .Select(x => x.TeamId!.Value)
                .Distinct()
                .ToListAsync(cancellationToken)
            : [];

        return new MatchAdministrationAccess(userId, isAdmin, isModerator, isContributor, assignedTeamIds);
    }
}
