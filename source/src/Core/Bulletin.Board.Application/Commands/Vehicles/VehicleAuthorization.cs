using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;

namespace Bulletin.Board.Application.Commands.Vehicles;

/// <summary>
/// Shared authorization helper for vehicle commands. Centralises three checks
/// every mutation needs:
/// 1. User authenticated.
/// 2. User owns the listing (via Provider.UserId) OR is Admin.
/// 3. The listing's category is "car-rental".
/// </summary>
internal static class VehicleAuthorization
{
    private const string AdminRole = "Admin";
    private const string CarRentalSlug = "car-rental";

    public static async Task<Listing> AuthorizeListingMutationAsync(
        IListingRepository listings,
        ICurrentUserService currentUser,
        Guid listingId,
        CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new UnauthorizedAccessException("User must be authenticated.");

        var listing = await listings.GetAdminDetailAsync(listingId, ct)
            ?? throw new InvalidOperationException("Listing not found.");

        if (listing.IsDeleted)
            throw new InvalidOperationException("Listing has been deleted.");

        if (!currentUser.IsInRole(AdminRole))
        {
            if (listing.Provider is null || listing.Provider.UserId != userId)
                throw new UnauthorizedAccessException("Only the listing owner can mutate its vehicles.");
        }

        if (!string.Equals(listing.Category?.Slug, CarRentalSlug, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                $"Vehicles can only be managed for listings in the '{CarRentalSlug}' category.");

        return listing;
    }
}
