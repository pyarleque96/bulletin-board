namespace Bulletin.Board.Domain.Enums;

public enum UnavailabilityReason
{
    /// <summary>Marked unavailable because a reservation request was accepted.</summary>
    Reserved,

    /// <summary>Vehicle is in maintenance / repair.</summary>
    Maintenance,

    /// <summary>Provider blocked the dates manually for any other reason.</summary>
    Manual
}
