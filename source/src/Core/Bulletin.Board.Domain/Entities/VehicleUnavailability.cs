using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Exceptions;

namespace Bulletin.Board.Domain.Entities;

/// <summary>
/// A blackout range during which a vehicle cannot be rented. Default rental model:
/// "available unless blocked" — the public availability viewer renders rangos blocked
/// in red and everything else as free.
/// <para>
/// PostgreSQL enforces non-overlap at the storage layer via an EXCLUDE constraint
/// (<c>btree_gist</c> + <c>daterange</c> &amp;&amp;), so two blackouts cannot collide
/// even if the application sends concurrent writes.
/// </para>
/// </summary>
public class VehicleUnavailability
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid VehicleId { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public UnavailabilityReason Reason { get; private set; }

    /// <summary>
    /// FK to <c>reservation_requests.id</c> when the blackout was created by an
    /// accepted reservation. Null otherwise (Maintenance, Manual). The reservation
    /// table itself is added in a later migration; the column is created up-front
    /// to avoid future schema churn.
    /// </summary>
    public Guid? ReservationId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public Vehicle Vehicle { get; private set; } = null!;

    private VehicleUnavailability() { }

    public static VehicleUnavailability Create(
        Guid vehicleId,
        DateOnly startDate,
        DateOnly endDate,
        UnavailabilityReason reason,
        Guid? reservationId = null)
    {
        if (endDate < startDate)
            throw new DomainException("End date cannot be before start date.");

        if (reason == UnavailabilityReason.Reserved && !reservationId.HasValue)
            throw new DomainException("Reserved blackouts must reference a reservation.");
        if (reason != UnavailabilityReason.Reserved && reservationId.HasValue)
            throw new DomainException("Only Reserved blackouts may reference a reservation.");

        return new VehicleUnavailability
        {
            VehicleId = vehicleId,
            StartDate = startDate,
            EndDate = endDate,
            Reason = reason,
            ReservationId = reservationId
        };
    }

    public bool Covers(DateOnly date) => date >= StartDate && date <= EndDate;
}
