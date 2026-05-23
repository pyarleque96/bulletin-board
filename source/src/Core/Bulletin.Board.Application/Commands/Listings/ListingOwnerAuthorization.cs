using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;

namespace Bulletin.Board.Application.Commands.Listings;

/// <summary>
/// Shared authorization helper for listing commands that only the owner may perform.
/// </summary>
public static class ListingOwnerAuthorization
{
    private const string AdminRole = "Admin";

    /// <summary>
    /// Loads the listing (with Provider nav) and verifies the caller is its owner or Admin.
    /// Returns the listing for chaining.
    /// </summary>
    public static async Task<Listing> AuthorizeAsync(
        IListingRepository listings,
        ICurrentUserService currentUser,
        Guid listingId,
        CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new UnauthorizedAccessException("User must be authenticated.");

        // GetAdminDetailAsync loads Provider without visibility filters (needed for paused listings).
        var listing = await listings.GetAdminDetailAsync(listingId, ct)
            ?? throw new InvalidOperationException("Listing not found.");

        if (listing.IsDeleted)
            throw new InvalidOperationException("Listing has been deleted.");

        if (!currentUser.IsInRole(AdminRole))
        {
            if (listing.Provider is null || listing.Provider.UserId != userId)
                throw new UnauthorizedAccessException("Only the listing owner can perform this action.");
        }

        return listing;
    }

    /// <summary>
    /// In-process authorization check against an already-loaded listing entity.
    /// </summary>
    public static void AuthorizeOwner(Listing listing, Guid userId, ICurrentUserService currentUser)
    {
        if (currentUser.IsInRole(AdminRole)) return;

        if (listing.Provider is null || listing.Provider.UserId != userId)
            throw new UnauthorizedAccessException("Only the listing owner can perform this action.");
    }
}
