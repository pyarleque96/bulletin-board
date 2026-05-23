using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Exceptions;

namespace Bulletin.Board.Domain.Entities;

/// <summary>
/// A pre-WhatsApp inquiry from a client to a provider. Captures language, subject and
/// message preview. Tied to a <see cref="Listing"/> and optionally to a property/vehicle/etc.
/// Por regla #8, cualquier paso de "Contacted" debe registrar un <see cref="ContactLog"/>
/// y disparar email al GM.
/// </summary>
public class Inquiry
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ListingId { get; private set; }
    public Guid ProviderId { get; private set; }
    public Guid? InitiatorUserId { get; private set; }
    public string ClientName { get; private set; } = string.Empty;
    public string ClientWhatsAppPhone { get; private set; } = string.Empty;
    public string ClientLanguage { get; private set; } = "en";
    public string Subject { get; private set; } = string.Empty;
    public string MessagePreview { get; private set; } = string.Empty;
    public InquiryStatus Status { get; private set; } = InquiryStatus.New;
    public bool IsRead { get; private set; }

    /// <summary>
    /// Optional reference to a sub-entity for category-specific contexts
    /// (e.g. a property in housing, a vehicle in car-rental). Nullable for generic listings.
    /// </summary>
    public Guid? RelatedEntityId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ContactedAt { get; private set; }

    public Listing Listing { get; private set; } = null!;
    public Provider Provider { get; private set; } = null!;
    public ICollection<ContactLog> ContactLogs { get; private set; } = [];

    private Inquiry() { }

    public static Inquiry Create(Guid listingId, Guid providerId, Guid? initiatorUserId,
        string clientName, string clientWhatsAppPhone, string clientLanguage,
        string subject, string messagePreview, Guid? relatedEntityId = null)
    {
        if (string.IsNullOrWhiteSpace(clientName))
            throw new DomainException("Client name cannot be empty.");
        if (string.IsNullOrWhiteSpace(clientWhatsAppPhone))
            throw new DomainException("Client WhatsApp phone cannot be empty.");

        return new Inquiry
        {
            ListingId = listingId,
            ProviderId = providerId,
            InitiatorUserId = initiatorUserId,
            ClientName = clientName.Trim(),
            ClientWhatsAppPhone = clientWhatsAppPhone.Trim(),
            ClientLanguage = string.IsNullOrWhiteSpace(clientLanguage) ? "en" : clientLanguage.Trim().ToLowerInvariant(),
            Subject = subject?.Trim() ?? string.Empty,
            MessagePreview = messagePreview?.Trim() ?? string.Empty,
            RelatedEntityId = relatedEntityId
        };
    }

    public void MarkRead()
    {
        IsRead = true;
        if (Status == InquiryStatus.New)
            Status = InquiryStatus.Read;
    }

    public ContactLog MarkContacted(ContactMethod method, string? note = null)
    {
        Status = InquiryStatus.Contacted;
        ContactedAt ??= DateTimeOffset.UtcNow;
        var log = ContactLog.Create(Id, ListingId, method, note);
        ContactLogs.Add(log);
        return log;
    }

    public void Close()
    {
        Status = InquiryStatus.Closed;
    }
}
