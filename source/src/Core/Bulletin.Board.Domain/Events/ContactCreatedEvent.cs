namespace Bulletin.Board.Domain.Events;

public record ContactCreatedEvent(Guid ContactId, Guid ListingId, Guid ProviderId, Guid InitiatorUserId);
