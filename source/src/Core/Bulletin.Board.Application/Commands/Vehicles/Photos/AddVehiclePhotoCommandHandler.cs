using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Commands.Vehicles.Photos;

public sealed class AddVehiclePhotoCommandHandler(
    IListingRepository listings,
    IVehicleRepository vehicles,
    ICurrentUserService currentUser,
    ILogger<AddVehiclePhotoCommandHandler> logger)
    : IRequestHandler<AddVehiclePhotoCommand, Guid>
{
    public async Task<Guid> Handle(AddVehiclePhotoCommand request, CancellationToken ct)
    {
        var vehicle = await vehicles.GetByIdAsync(request.VehicleId, ct)
            ?? throw new InvalidOperationException("Vehicle not found.");

        var listing = await VehicleAuthorization.AuthorizeListingMutationAsync(
            listings, currentUser, vehicle.ListingId, ct);

        // If the new photo is primary, demote the existing primary first to keep the
        // "exactly one primary per vehicle" invariant true.
        if (request.IsPrimary)
            await vehicles.UnsetPrimaryForAllPhotosAsync(vehicle.Id, ct);

        var photo = VehiclePhoto.Create(
            vehicleId: vehicle.Id,
            filePath: request.FilePath,
            displayOrder: request.DisplayOrder,
            isPrimary: request.IsPrimary);

        await vehicles.AddPhotoAsync(photo, ct);
        listing.RequireReapproval();
        await vehicles.SaveChangesAsync(ct);

        logger.LogInformation(
            "Vehicle photo {PhotoId} added to vehicle {VehicleId}; listing {ListingId} reverted to Pending.",
            photo.Id, vehicle.Id, listing.Id);

        return photo.Id;
    }
}
