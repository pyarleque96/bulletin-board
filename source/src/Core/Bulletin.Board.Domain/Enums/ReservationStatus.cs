namespace Bulletin.Board.Domain.Enums;

public enum ReservationStatus
{
    /// <summary>Submitted by the requester, awaiting the provider's decision.</summary>
    Pending,

    /// <summary>Provider accepted; a Reserved blackout was created on the vehicle.</summary>
    Accepted,

    /// <summary>Provider rejected. Terminal — requester must submit a new request.</summary>
    Rejected,

    /// <summary>Cancelled by either side. If was Accepted the linked blackout is removed.</summary>
    Cancelled,

    /// <summary>Service was rendered. Terminal — set by the listing owner from Accepted state.</summary>
    Completed
}
