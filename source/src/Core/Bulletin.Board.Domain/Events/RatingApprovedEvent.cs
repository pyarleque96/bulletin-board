namespace Bulletin.Board.Domain.Events;

public record RatingApprovedEvent(Guid RatingId, Guid ListingId, Guid ProviderId, short Stars);
