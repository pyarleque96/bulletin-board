namespace Bulletin.Board.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid? ActorUserId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string ResourceType { get; private set; } = string.Empty;
    public Guid? ResourceId { get; private set; }
    public string? ChangesJson { get; private set; }
    public string? IpAddress { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    private AuditLog() { }

    public static AuditLog Create(string action, string resourceType, Guid? actorUserId = null,
        Guid? resourceId = null, string? changesJson = null, string? ipAddress = null)
        => new()
        {
            Action = action,
            ResourceType = resourceType,
            ActorUserId = actorUserId,
            ResourceId = resourceId,
            ChangesJson = changesJson,
            IpAddress = ipAddress
        };
}
