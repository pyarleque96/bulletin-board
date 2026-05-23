using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Commands.Listings;

public sealed class DeleteOwnListingCommandHandler(
    IListingRepository listings,
    ICurrentUserService currentUser,
    ILogger<DeleteOwnListingCommandHandler> logger)
    : IRequestHandler<DeleteOwnListingCommand>
{
    public async Task Handle(DeleteOwnListingCommand request, CancellationToken ct)
    {
        var listing = await ListingOwnerAuthorization.AuthorizeAsync(
            listings, currentUser, request.ListingId, ct);

        listing.SoftDelete();

        await listings.SaveChangesAsync(ct);

        logger.LogInformation(
            "Listing {ListingId} soft-deleted by owner {UserId}.",
            request.ListingId, currentUser.UserId);
    }
}
