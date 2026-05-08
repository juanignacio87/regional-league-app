namespace RegionalLeagueApp.Infrastructure.Seed;

public sealed class DevelopmentSeedOptions
{
    public bool Enabled { get; set; }
    public string AdminEmail { get; set; } = "admin@regional.test";
    public string AdminDisplayName { get; set; } = "Admin Regional";

    public string AdminPassword { get; set; } = "Admin123!";
}
