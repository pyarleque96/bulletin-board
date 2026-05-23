using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Commands.Listings;

public sealed class SetListingActiveCommandHandler(
    IListingRepository listings,
    ICurrentUserService currentUser,
    ILogger<SetListingActiveCommandHandler> logger)
    : IRequestHandler<SetListingActiveCommand>
{
    public async Task Handle(SetListingActiveCommand request, CancellationToken ct)
    {
        var listing = await ListingOwnerAuthorization.AuthorizeAsync(
            listings, currentUser, request.ListingId, ct);

        if (request.IsActive)
            listing.ResumeByOwner();
        else
            listing.PauseByOwner();

        await listings.SaveChangesAsync(ct);

        logger.LogInformation(
            "Listing {ListingId} {Action} by user {UserId}.",
            request.ListingId,
            request.IsActive ? "resumed" : "paused",
            currentUser.UserId);
    }
}
