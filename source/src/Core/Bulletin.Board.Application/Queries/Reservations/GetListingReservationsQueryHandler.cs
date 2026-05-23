using Bulletin.Board.Application.Commands.Vehicles;
using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using MediatR;

namespace Bulletin.Board.Application.Queries.Reservations;

public sealed class GetListingReservationsQueryHandler(
    IReservationRequestRepository reservations,
    IListingRepository listings,
    ICurrentUserService currentUser)
    : IRequestHandler<GetListingReservationsQuery, PagedResult<ReservationRequestDto>>
{
    public async Task<PagedResult<ReservationRequestDto>> Handle(
        GetListingReservationsQuery request, CancellationToken ct)
    {
        // Reuse vehicle-level authorization: same check (owner OR admin) + car-rental category.
        await VehicleAuthorization.AuthorizeListingMutationAsync(
            listings, currentUser, request.ListingId, ct);

        ReservationStatus? statusFilter = null;
        if (!string.IsNullOrWhiteSpace(request.Status)
            && Enum.TryParse<ReservationStatus>(request.Status, ignoreCase: true, out var parsed))
            statusFilter = parsed;

        var paged = new PagedRequest(request.Page, request.PageSize);

        var (entities, total, hasAnyUnfiltered) = await reservations.GetForListingPagedAsync(
            request.ListingId, statusFilter, paged.Skip, paged.Take, ct);

        var items = entities
            .Select(r => new ReservationRequestDto(
                Id: r.Id,
                VehicleId: r.VehicleId,
                VehicleName: r.Vehicle?.Name ?? string.Empty,
                ListingId: r.ListingId,
                RequesterName: r.RequesterName,
                RequesterEmail: r.RequesterEmail,
                RequesterPhone: r.RequesterPhone,
                StartDate: r.StartDate,
                EndDate: r.EndDate,
                Comment: r.Comment,
                Status: r.Status.ToString(),
                ResponseNote: r.ResponseNote,
                CreatedAt: r.CreatedAt,
                RespondedAt: r.RespondedAt))
            .ToList();

        return new PagedResult<ReservationRequestDto>(items, paged.Page, paged.Take, total, hasAnyUnfiltered);
    }
}
