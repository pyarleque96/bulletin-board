using Bulletin.Board.Application.Snapshots;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Commands.Vehicles.Photos;

public sealed class SetVehiclePhotoVisibilityCommandHandler(
    IVehicleRepository vehicles,
    IListingSnapshotRefresher snapshotRefresher,
    ICurrentUserService currentUser,
    ILogger<SetVehiclePhotoVisibilityCommandHandler> logger)
    : IRequestHandler<SetVehiclePhotoVisibilityCommand>
{
    public async Task Handle(SetVehiclePhotoVisibilityCommand request, CancellationToken ct)
    {
        // Admin-only — caller authorization is also enforced at the controller via [Authorize(Roles="Admin")],
        // but defense in depth keeps the handler independently safe.
        if (!currentUser.IsInRole("Admin"))
            throw new UnauthorizedAccessException("Only admins can change photo visibility.");

        var photo = await vehicles.GetPhotoByIdAsync(request.PhotoId, ct)
            ?? throw new InvalidOperationException("Photo not found.");

        if (request.IsPublic) photo.MakePublic();
        else                   photo.MakePrivate();

        await vehicles.SaveChangesAsync(ct);

        logger.LogInformation(
            "Vehicle photo {PhotoId} visibility set to IsPublic={IsPublic}.",
            photo.Id, request.IsPublic);

        // Visibility flip on an Approved listing changes what public reads see. Regenerate
        // the snapshot so the change takes effect immediately (no need to re-approve).
        // No-op for listings not in Approved status.
        await snapshotRefresher.RefreshIfApprovedAsync(photo.Vehicle.ListingId, ct);
    }
}
