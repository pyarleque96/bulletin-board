using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Exceptions;

namespace Bulletin.Board.Domain.Entities;

/// <summary>
/// A reservation request submitted by an authenticated user against a specific vehicle.
/// State machine: Pending -> Accepted | Rejected | Cancelled (all terminal).
/// Requester contact info is snapshotted on creation so historical records survive
/// later profile edits.
/// </summary>
public class ReservationRequest
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid VehicleId { get; private set; }
    public Guid ListingId { get; private set; }      // denormalized for listing-level owner queries
    public Guid RequesterUserId { get; private set; }

    public string RequesterName { get; private set; } = string.Empty;
    public string RequesterEmail { get; private set; } = string.Empty;
    public string? RequesterPhone { get; private set; }

    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public string? Comment { get; private set; }

    public ReservationStatus Status { get; private set; } = ReservationStatus.Pending;
    public string? ResponseNote { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? RespondedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    public Vehicle Vehicle { get; private set; } = null!;
    public Listing Listing { get; private set; } = null!;

    private ReservationRequest() { }

    public static ReservationRequest Create(
        Guid vehicleId,
        Guid listingId,
        Guid requesterUserId,
        string requesterName,
        string requesterEmail,
        string? requesterPhone,
        DateOnly startDate,
        DateOnly endDate,
        string? comment)
    {
        if (string.IsNullOrWhiteSpace(requesterName))
            throw new DomainException("Requester name is required.");
        if (string.IsNullOrWhiteSpace(requesterEmail))
            throw new DomainException("Requester email is required.");
        if (endDate < startDate)
            throw new DomainException("End date cannot be before start date.");
        if (startDate < DateOnly.FromDateTime(DateTime.UtcNow.Date))
            throw new DomainException("Start date cannot be in the past.");
        if (comment?.Length > 1000)
            throw new DomainException("Comment must be 1000 characters or less.");

        return new ReservationRequest
        {
            VehicleId = vehicleId,
            ListingId = listingId,
            RequesterUserId = requesterUserId,
            RequesterName = requesterName.Trim(),
            RequesterEmail = requesterEmail.Trim(),
            RequesterPhone = requesterPhone?.Trim(),
            StartDate = startDate,
            EndDate = endDate,
            Comment = comment?.Trim()
        };
    }

    public void Accept(string? note = null)
    {
        if (Status != ReservationStatus.Pending)
            throw new DomainException($"Cannot accept a reservation in {Status} state.");
        Status = ReservationStatus.Accepted;
        ResponseNote = note?.Trim();
        RespondedAt = DateTimeOffset.UtcNow;
    }

    public void Reject(string? note = null)
    {
        if (Status != ReservationStatus.Pending)
            throw new DomainException($"Cannot reject a reservation in {Status} state.");
        Status = ReservationStatus.Rejected;
        ResponseNote = note?.Trim();
        RespondedAt = DateTimeOffset.UtcNow;
    }

    public void Cancel(string? note = null)
    {
        if (Status == ReservationStatus.Cancelled)
            return;
        if (Status == ReservationStatus.Rejected)
            throw new DomainException("Rejected reservations cannot be cancelled.");
        Status = ReservationStatus.Cancelled;
        ResponseNote = note?.Trim();
        RespondedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Marks the reservation as completed. Only valid from <see cref="ReservationStatus.Accepted"/>.
    /// Called by the listing owner after the service was rendered.
    /// </summary>
    public void MarkCompleted()
    {
        if (Status != ReservationStatus.Accepted)
            throw new DomainException($"Cannot mark a reservation as completed from {Status} state. Only Accepted reservations can be completed.");
        Status = ReservationStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
    }
}
