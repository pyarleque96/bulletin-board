namespace Bulletin.Board.Domain.Enums;

/// <summary>
/// Moderation status for listing photos (regla #4 — GM decide qué fotos son públicas).
/// Default at creation is <see cref="Pending"/>. Only Admin moves to Public / Hidden / Rejected.
/// </summary>
public enum PhotoStatus
{
    Pending,
    Public,
    Hidden,
    Rejected
}
