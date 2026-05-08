namespace RegionalLeagueApp.Infrastructure.Identity;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Moderator = "Moderator";
    public const string Contributor = "Contributor";

    public static readonly string[] All = [Admin, Moderator, Contributor];
}
