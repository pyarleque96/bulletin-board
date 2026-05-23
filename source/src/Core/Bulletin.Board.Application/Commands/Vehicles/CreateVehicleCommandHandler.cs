using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Commands.Vehicles;

public sealed class CreateVehicleCommandHandler(
    IListingRepository listings,
    IVehicleRepository vehicles,
    ICurrentUserService currentUser,
    ILogger<CreateVehicleCommandHandler> logger)
    : IRequestHandler<CreateVehicleCommand, Guid>
{
    public async Task<Guid> Handle(CreateVehicleCommand request, CancellationToken ct)
    {
        var listing = await VehicleAuthorization.AuthorizeListingMutationAsync(
            listings, currentUser, request.ListingId, ct);

        if (!Enum.TryParse<Transmission>(request.Transmission, ignoreCase: true, out var transmission))
            throw new ArgumentException($"Invalid transmission '{request.Transmission}'.");

        var vehicle = Vehicle.Create(
            listingId: listing.Id,
            name: request.Name,
            passengerMax: request.PassengerMax,
            transmission: transmission,
            hasAirConditioning: request.HasAirConditioning,
            dailyRateCents: request.DailyRateCents,
            weeklyRateCents: request.WeeklyRateCents,
            monthlyRateCents: request.MonthlyRateCents,
            displayOrder: request.DisplayOrder);

        await vehicles.AddAsync(vehicle, ct);
        listing.RequireReapproval();
        await vehicles.SaveChangesAsync(ct);

        logger.LogInformation(
            "Vehicle {VehicleId} created for listing {ListingId}; status reverted to Pending.",
            vehicle.Id, listing.Id);

        return vehicle.Id;
    }
}
