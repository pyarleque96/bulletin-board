namespace Bulletin.Board.Domain.Events;

public record ListingApprovedEvent(Guid ListingId, Guid ProviderId, Guid ApproverId);
