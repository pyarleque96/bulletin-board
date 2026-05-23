using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Commands.Vehicles.Availability;

public sealed class AddVehicleUnavailabilityCommandHandler(
    IListingRepository listings,
    IVehicleRepository vehicles,
    ICurrentUserService currentUser,
    ILogger<AddVehicleUnavailabilityCommandHandler> logger)
    : IRequestHandler<AddVehicleUnavailabilityCommand, Guid>
{
    public async Task<Guid> Handle(AddVehicleUnavailabilityCommand request, CancellationToken ct)
    {
        var vehicle = await vehicles.GetByIdAsync(request.VehicleId, ct)
            ?? throw new InvalidOperationException("Vehicle not found.");

        await VehicleAuthorization.AuthorizeListingMutationAsync(
            listings, currentUser, vehicle.ListingId, ct);

        if (!Enum.TryParse<UnavailabilityReason>(request.Reason, ignoreCase: true, out var reason))
            throw new ArgumentException($"Invalid reason '{request.Reason}'.");

        // Reserved blackouts are created by the reservation flow only — never by manual blocking.
        if (reason == UnavailabilityReason.Reserved)
            throw new InvalidOperationException(
                "Reserved blackouts are managed by the reservation system; use Manual or Maintenance.");

        var blackout = VehicleUnavailability.Create(
            vehicleId: vehicle.Id,
            startDate: request.StartDate,
            endDate: request.EndDate,
            reason: reason);

        // Repo translates the EXCLUDE-constraint violation into VehicleUnavailabilityOverlapException
        // so the controller can return a clean 409.
        await vehicles.AddUnavailabilityAsync(blackout, ct);

        // Note: blocking days does NOT require GM re-approval — operational toggle, not a content edit.
        logger.LogInformation(
            "Blackout {Id} added to vehicle {VehicleId}: {Start} to {End} ({Reason}).",
            blackout.Id, vehicle.Id, request.StartDate, request.EndDate, reason);

        return blackout.Id;
    }
}
