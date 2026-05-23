using Bulletin.Board.Application.DTOs;
using MediatR;

namespace Bulletin.Board.Application.Queries.Listings;

public record SearchListingsQuery(
    string? Category,
    string? Tier,
    decimal? MinRating,
    string? Language,
    int Page = 1,
    int PageSize = 12
) : IRequest<PagedResult<ListingCardDto>>;
