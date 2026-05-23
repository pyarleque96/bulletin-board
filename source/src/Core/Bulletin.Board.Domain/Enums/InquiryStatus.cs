namespace Bulletin.Board.Domain.Enums;

/// <summary>
/// Track-state of an inquiry from a client to the provider. Distinct from a <see cref="ContactStatus"/>
/// to allow follow-up flows (read receipts, manual mark-as-contacted via WhatsApp).
/// </summary>
public enum InquiryStatus
{
    New,
    Read,
    Contacted,
    Closed
}
