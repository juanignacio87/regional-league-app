using RegionalLeagueApp.Domain.Common;

namespace RegionalLeagueApp.Domain.Collaboration;

public sealed class AuditLog : Entity
{
    public string EntityName { get; private set; } = string.Empty;
    public string EntityId { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public Guid? UserId { get; private set; }
    public string? Details { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    private AuditLog()
    {
    }

    public AuditLog(string entityName, string entityId, string action, Guid? userId = null)
    {
        EntityName = entityName;
        EntityId = entityId;
        Action = action;
        UserId = userId;
    }
}
