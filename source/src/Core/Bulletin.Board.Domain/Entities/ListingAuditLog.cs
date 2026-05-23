namespace Bulletin.Board.Domain.Entities;

/// <summary>
/// Append-only audit trail of listing state transitions driven by the provider
/// (regla #5 — cualquier edición regresa a Pending). Distinto del <see cref="AuditLog"/>
/// genérico que captura acciones administrativas.
/// </summary>
public class ListingAuditLog
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ListingId { get; private set; }
    public Guid? ActorUserId { get; private set; }

    /// <summary>
    /// Symbolic event name: "ListingEdited", "VehicleEdited", "PhotoModerated", "ApprovedByGm", etc.
    /// </summary>
    public string Event { get; private set; } = string.Empty;

    public string? PreviousStatus { get; private set; }
    public string? NewStatus { get; private set; }
    public string? Notes { get; private set; }

    public DateTimeOffset OccurredAt { get; private set; } = DateTimeOffset.UtcNow;

    public Listing Listing { get; private set; } = null!;

    private ListingAuditLog() { }

    public static ListingAuditLog Record(Guid listingId, string @event,
        Guid? actorUserId = null, string? previousStatus = null, string? newStatus = null, string? notes = null)
        => new()
        {
            ListingId = listingId,
            Event = @event,
            ActorUserId = actorUserId,
            PreviousStatus = previousStatus,
            NewStatus = newStatus,
            Notes = notes
        };
}
