using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using MediatR;

namespace Bulletin.Board.Application.Queries.Reservations;

public sealed class GetMyReservationsQueryHandler(
    IReservationRequestRepository reservations,
    ICurrentUserService currentUser)
    : IRequestHandler<GetMyReservationsQuery, IReadOnlyList<MyReservationDto>>
{
    public async Task<IReadOnlyList<MyReservationDto>> Handle(
        GetMyReservationsQuery request, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new UnauthorizedAccessException("User must be authenticated.");

        var entities = await reservations.GetForUserAsync(userId, ct);

        return entities
            .Select(r => new MyReservationDto(
                Id: r.Id,
                VehicleId: r.VehicleId,
                VehicleName: r.Vehicle?.Name ?? string.Empty,
                ListingId: r.ListingId,
                ListingTitleEn: r.Listing?.TitleEn ?? string.Empty,
                ListingTitleEs: r.Listing?.TitleEs ?? string.Empty,
                StartDate: r.StartDate,
                EndDate: r.EndDate,
                Comment: r.Comment,
                Status: r.Status.ToString(),
                ResponseNote: r.ResponseNote,
                CreatedAt: r.CreatedAt,
                RespondedAt: r.RespondedAt))
            .ToList();
    }
}
