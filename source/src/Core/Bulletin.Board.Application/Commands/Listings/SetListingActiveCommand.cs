using MediatR;

namespace Bulletin.Board.Application.Commands.Listings;

/// <summary>
/// Pauses or resumes a listing. Owner-only. Does NOT trigger re-approval (not a content edit).
/// </summary>
public record SetListingActiveCommand(Guid ListingId, bool IsActive) : IRequest;
