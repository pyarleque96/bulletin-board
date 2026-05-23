using Bulletin.Board.Application.DTOs;
using MediatR;

namespace Bulletin.Board.Application.Queries.Listings;

/// <summary>Owner-only. Returns a paginated list of approved ratings for a listing.</summary>
public record GetListingReviewsQuery(Guid ListingId, int Page = 1, int PageSize = 10)
    : IRequest<PagedResult<ListingReviewDto>>;
