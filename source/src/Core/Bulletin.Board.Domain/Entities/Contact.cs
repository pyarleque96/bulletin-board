using Bulletin.Board.Domain.Events;

namespace Bulletin.Board.Domain.Entities;

public class Contact
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ListingId { get; private set; }
    public Guid InitiatorUserId { get; private set; }
    public Guid ProviderId { get; private set; }
    public string Channel { get; private set; } = "WhatsApp";
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public Listing Listing { get; private set; } = null!;
    public Provider Provider { get; private set; } = null!;
    public Rating? Rating { get; private set; }

    private readonly List<object> _domainEvents = [];
    public IReadOnlyList<object> DomainEvents => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();

    private Contact() { }

    public static Contact Create(Guid listingId, Guid initiatorUserId, Guid providerId)
    {
        var contact = new Contact
        {
            ListingId = listingId,
            InitiatorUserId = initiatorUserId,
            ProviderId = providerId
        };
        contact._domainEvents.Add(new ContactCreatedEvent(contact.Id, listingId, providerId, initiatorUserId));
        return contact;
    }
}
