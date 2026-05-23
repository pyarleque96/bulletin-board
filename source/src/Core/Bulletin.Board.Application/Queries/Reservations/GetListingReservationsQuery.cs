using Bulletin.Board.Application.DTOs;
using MediatR;

namespace Bulletin.Board.Application.Queries.Reservations;

public record GetListingReservationsQuery(
    Guid ListingId,
    string? Status,   // optional filter — null returns all
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<ReservationRequestDto>>;
