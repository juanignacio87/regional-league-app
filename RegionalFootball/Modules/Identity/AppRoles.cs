namespace RegionalFootball.Modules.Identity;

public static class AppRoles
{
    public const string Visitor = "Visitor";
    public const string Contributor = "Contributor";
    public const string Moderator = "Moderator";
    public const string Admin = "Admin";

    public static readonly string[] All = [Visitor, Contributor, Moderator, Admin];
}
