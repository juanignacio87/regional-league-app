namespace RegionalLeagueApp.Infrastructure.Seed;

public sealed class DevelopmentSeedOptions
{
    public bool Enabled { get; set; }
    public string AdminEmail { get; set; } = "admin@regional.test";
    public string AdminDisplayName { get; set; } = "Admin Regional";

    public string AdminPassword { get; set; } = "Admin123!";

    public string ModeratorEmail { get; set; } = "moderator@regional.test";
    public string ModeratorDisplayName { get; set; } = "Moderator Regional";
    public string ModeratorPassword { get; set; } = "Moderator123!";

    public string ContributorEmail { get; set; } = "contributor@regional.test";
    public string ContributorDisplayName { get; set; } = "Contributor Regional";
    public string ContributorPassword { get; set; } = "Contributor123!";
}
