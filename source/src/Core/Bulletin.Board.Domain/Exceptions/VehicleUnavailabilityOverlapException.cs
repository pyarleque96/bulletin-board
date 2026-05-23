namespace Bulletin.Board.Domain.Exceptions;

/// <summary>
/// Thrown when a blackout range collides with an existing one for the same vehicle.
/// Translated from the PostgreSQL EXCLUDE constraint <c>excl_vehicle_unavailability_no_overlap</c>
/// so callers get a meaningful error instead of a generic DB failure.
/// </summary>
public sealed class VehicleUnavailabilityOverlapException(string message)
    : DomainException(message);
