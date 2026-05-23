using Bulletin.Board.Application.DTOs;
using MediatR;

namespace Bulletin.Board.Application.Queries.Listings;

public record GetListingDetailBySlugQuery(string Slug) : IRequest<ListingDetailDto?>;
