using Bulletin.Board.Application.Commands.Vehicles;
using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using MediatR;

namespace Bulletin.Board.Application.Queries.Vehicles;

public sealed class GetOwnerVehiclesQueryHandler(
    IListingRepository listings,
    IVehicleRepository vehicles,
    ICurrentUserService currentUser)
    : IRequestHandler<GetOwnerVehiclesQuery, PagedResult<OwnerVehicleDto>>
{
    public async Task<PagedResult<OwnerVehicleDto>> Handle(
        GetOwnerVehiclesQuery request, CancellationToken ct)
    {
        // Reuses the same ownership + category check as the mutation handlers — the
        // list endpoint must be at least as restrictive as the create endpoint.
        await VehicleAuthorization.AuthorizeListingMutationAsync(
            listings, currentUser, request.ListingId, ct);

        var paged = new PagedRequest(request.Page, request.PageSize);

        var (entities, total) = await vehicles.GetByListingForOwnerPagedAsync(
            request.ListingId, paged.Skip, paged.Take, ct);

        var items = entities
            .Select(v => new OwnerVehicleDto(
                Id: v.Id,
                Name: v.Name,
                PassengerMax: v.PassengerMax,
                Transmission: v.Transmission.ToString(),
                HasAirConditioning: v.HasAirConditioning,
                DailyRateCents: v.DailyRateCents,
                WeeklyRateCents: v.WeeklyRateCents,
                MonthlyRateCents: v.MonthlyRateCents,
                DisplayOrder: v.DisplayOrder,
                IsActive: v.IsActive,
                Photos: v.Photos
                    .OrderByDescending(p => p.IsPrimary)
                    .ThenBy(p => p.DisplayOrder)
                    .Select(p => new OwnerVehiclePhotoDto(
                        p.Id, p.FilePath, p.IsPrimary, p.IsPublic, p.DisplayOrder))
                    .ToArray()))
            .ToList();

        return new PagedResult<OwnerVehicleDto>(items, paged.Page, paged.Take, total);
    }
}
