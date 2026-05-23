using Bulletin.Board.Application.DTOs;
using MediatR;

namespace Bulletin.Board.Application.Queries.Listings;

public record GetMyListingsQuery(int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<MyListingDto>>;
