using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Commands.Vehicles;

public sealed class UpdateVehicleCommandHandler(
    IListingRepository listings,
    IVehicleRepository vehicles,
    ICurrentUserService currentUser,
    ILogger<UpdateVehicleCommandHandler> logger)
    : IRequestHandler<UpdateVehicleCommand>
{
    public async Task Handle(UpdateVehicleCommand request, CancellationToken ct)
    {
        var vehicle = await vehicles.GetByIdAsync(request.VehicleId, ct)
            ?? throw new InvalidOperationException("Vehicle not found.");

        var listing = await VehicleAuthorization.AuthorizeListingMutationAsync(
            listings, currentUser, vehicle.ListingId, ct);

        if (!Enum.TryParse<Transmission>(request.Transmission, ignoreCase: true, out var transmission))
            throw new ArgumentException($"Invalid transmission '{request.Transmission}'.");

        vehicle.Edit(
            name: request.Name,
            passengerMax: request.PassengerMax,
            transmission: transmission,
            hasAirConditioning: request.HasAirConditioning,
            dailyRateCents: request.DailyRateCents,
            weeklyRateCents: request.WeeklyRateCents,
            monthlyRateCents: request.MonthlyRateCents,
            displayOrder: request.DisplayOrder);

        listing.RequireReapproval();
        await vehicles.SaveChangesAsync(ct);

        logger.LogInformation(
            "Vehicle {VehicleId} updated; listing {ListingId} reverted to Pending.",
            vehicle.Id, listing.Id);
    }
}
