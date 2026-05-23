using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Application.Queries.Listings;
using Bulletin.Board.Application.Snapshots;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Commands.Listings;

public sealed class UpdateOwnListingCommandHandler(
    IListingRepository listings,
    ICurrentUserService currentUser,
    IListingSnapshotBuilder snapshotBuilder,
    ILogger<UpdateOwnListingCommandHandler> logger)
    : IRequestHandler<UpdateOwnListingCommand, ListingDetailDto>
{
    public async Task<ListingDetailDto> Handle(UpdateOwnListingCommand request, CancellationToken ct)
    {
        var listing = await ListingOwnerAuthorization.AuthorizeAsync(
            listings, currentUser, request.ListingId, ct);

        // Regla #5: Listing.Edit() sets Status = Pending automatically.
        listing.Edit(
            request.TitleEn,
            request.TitleEs,
            request.DescriptionEn,
            request.DescriptionEs,
            request.Price,
            request.Location,
            request.WhatsAppNumber);

        await listings.SaveChangesAsync(ct);

        logger.LogInformation(
            "Listing {ListingId} edited by owner {UserId}. Status reverted to Pending (regla #5).",
            request.ListingId, currentUser.UserId);

        // Reload with all navigations so the DTO is fully populated.
        var reloaded = await listings.GetAdminDetailAsync(request.ListingId, ct)
            ?? throw new InvalidOperationException("Listing not found after save.");

        var snapshot = snapshotBuilder.Parse(reloaded.PublishedSnapshot);

        return await ListingDetailMapper.BuildAsync(reloaded, snapshot, listings, ct);
    }
}
