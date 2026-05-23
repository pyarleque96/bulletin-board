namespace Bulletin.Board.Domain.Enums;

/// <summary>
/// Moderation status for the provider's reply to a review (regla #7 — el reply también
/// pasa por moderación del GM antes de hacerse público).
/// </summary>
public enum ReviewReplyStatus
{
    PendingGm,
    Published,
    Rejected
}
