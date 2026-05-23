using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Domain.Interfaces.Repositories;
using MediatR;

namespace Bulletin.Board.Application.Queries.Vehicles;

public sealed class GetVehicleAvailabilityQueryHandler(IVehicleRepository vehicles)
    : IRequestHandler<GetVehicleAvailabilityQuery, VehicleAvailabilityDto?>
{
    public async Task<VehicleAvailabilityDto?> Handle(GetVehicleAvailabilityQuery request, CancellationToken ct)
    {
        // Public endpoint — no authorization. Anyone (auth or anonymous) can read availability
        // because it's part of the public detail layout for car-rental listings.
        var vehicle = await vehicles.GetByIdAsync(request.VehicleId, ct);
        if (vehicle is null || !vehicle.IsActive) return null;

        var blackouts = await vehicles.GetUnavailabilityInRangeAsync(
            request.VehicleId, request.From, request.To, ct);

        return new VehicleAvailabilityDto(
            VehicleId: request.VehicleId,
            From: request.From,
            To: request.To,
            Blackouts: blackouts
                .Select(u => new BlackoutRangeDto(u.Id, u.StartDate, u.EndDate, u.Reason.ToString()))
                .ToArray());
    }
}
