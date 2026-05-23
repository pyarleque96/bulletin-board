using Bulletin.Board.Domain.Enums;

namespace Bulletin.Board.Domain.Entities;

/// <summary>
/// Append-only log of every provider↔client contact action. Regla #8: cada interacción
/// genera email al GM. <see cref="GmNotifiedAt"/> se setea cuando el background job
/// confirma envío del email.
/// </summary>
public class ContactLog
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid InquiryId { get; private set; }
    public Guid ListingId { get; private set; }
    public ContactMethod Method { get; private set; }
    public string? Note { get; private set; }
    public DateTimeOffset ContactedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? GmNotifiedAt { get; private set; }

    public Inquiry Inquiry { get; private set; } = null!;

    private ContactLog() { }

    public static ContactLog Create(Guid inquiryId, Guid listingId, ContactMethod method, string? note = null)
        => new()
        {
            InquiryId = inquiryId,
            ListingId = listingId,
            Method = method,
            Note = note
        };

    public void MarkGmNotified() => GmNotifiedAt = DateTimeOffset.UtcNow;
}
