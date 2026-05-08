using System.Security.Claims;

namespace RegionalLeagueApp.Application.Abstractions.Identity;

public interface ICurrentUser
{
    Guid? UserId { get; }
    string? Email { get; }
    ClaimsPrincipal Principal { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
}
