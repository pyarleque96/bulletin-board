using Bulletin.Board.Domain.Enums;

namespace Bulletin.Board.Domain.Entities;

public class VipChangeLog
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public VipEntityType EntityType { get; private set; }
    public Guid EntityId { get; private set; }
    public VipChangeAction Action { get; private set; }
    public DateTimeOffset? PreviousVipUntil { get; private set; }
    public bool PreviousIsIndefinite { get; private set; }
    public DateTimeOffset? NewVipUntil { get; private set; }
    public bool NewIsIndefinite { get; private set; }
    public Guid ChangedByUserId { get; private set; }
    public DateTimeOffset ChangedAt { get; private set; } = DateTimeOffset.UtcNow;
    public string? IpAddress { get; private set; }
    public string? Reason { get; private set; }

    private VipChangeLog() { }

    public static VipChangeLog Record(
        VipEntityType entityType,
        Guid entityId,
        VipChangeAction action,
        DateTimeOffset? previousVipUntil,
        bool previousIsIndefinite,
        DateTimeOffset? newVipUntil,
        bool newIsIndefinite,
        Guid changedByUserId,
        string? ipAddress = null,
        string? reason = null)
        => new()
        {
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            PreviousVipUntil = previousVipUntil,
            PreviousIsIndefinite = previousIsIndefinite,
            NewVipUntil = newVipUntil,
            NewIsIndefinite = newIsIndefinite,
            ChangedByUserId = changedByUserId,
            IpAddress = ipAddress,
            Reason = reason
        };
}
