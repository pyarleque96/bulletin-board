using Bulletin.Board.Application.DTOs;
using MediatR;

namespace Bulletin.Board.Application.Queries.Listings;

/// <summary>
/// Owner-only query that returns full listing detail regardless of approval status.
/// Ownership is verified inside the handler; callers receive null (404) if the listing
/// does not exist or belongs to a different user — existence is never disclosed.
/// </summary>
public record GetMyListingDetailQuery(Guid ListingId) : IRequest<ListingDetailDto?>;
