using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using RegionalLeagueApp.Application.Abstractions.Identity;

namespace RegionalLeagueApp.Web.Services;

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public ClaimsPrincipal Principal => httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal(new ClaimsIdentity());

    public Guid? UserId
    {
        get
        {
            var value = Principal.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var userId) ? userId : null;
        }
    }

    public string? Email => Principal.FindFirstValue(ClaimTypes.Email);
    public bool IsAuthenticated => Principal.Identity?.IsAuthenticated == true;

    public bool IsInRole(string role) => Principal.IsInRole(role);
}
