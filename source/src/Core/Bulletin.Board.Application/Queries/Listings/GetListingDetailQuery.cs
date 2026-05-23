using Bulletin.Board.Application.DTOs;
using MediatR;

namespace Bulletin.Board.Application.Queries.Listings;

public record GetListingDetailQuery(Guid ListingId) : IRequest<ListingDetailDto?>;
