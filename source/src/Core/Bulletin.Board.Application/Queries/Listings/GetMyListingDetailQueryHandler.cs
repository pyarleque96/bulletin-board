using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Application.Snapshots;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Queries.Listings;

/// <summary>
/// Handles <see cref="GetMyListingDetailQuery"/>.
/// Returns the listing detail for its owner without the public visibility rule
/// (snapshot present OR status == Approved). A provider must always be able to see
/// their own listing — Pending, NeedsChanges or Rejected — in the owner panel.
///
/// Security contract:
///   - Returns null instead of throwing on ownership mismatch so the API maps to 404,
///     never leaking whether a listing with the given ID exists at all.
///   - Deleted listings are not surfaced (same as public endpoint).
/// </summary>
public sealed class GetMyListingDetailQueryHandler(
    IListingRepository listingRepository,
    IListingSnapshotBuilder snapshotBuilder,
    ICurrentUserService currentUser,
    ILogger<GetMyListingDetailQueryHandler> logger)
    : IRequestHandler<GetMyListingDetailQuery, ListingDetailDto?>
{
    public async Task<ListingDetailDto?> Handle(GetMyListingDetailQuery request, CancellationToken ct)
    {
        var userId = currentUser.UserId;
        if (userId is null)
        {
            logger.LogWarning("Unauthenticated request for owner listing detail {ListingId}", request.ListingId);
            return null;
        }

        logger.LogInformation(
            "Owner {UserId} fetching own listing detail for {ListingId}", userId, request.ListingId);

        // GetAdminDetailAsync loads Provider nav property without visibility filters.
        var listing = await listingRepository.GetAdminDetailAsync(request.ListingId, ct);

        if (listing is null || listing.IsDeleted)
            return null;

        // Ownership check — return null (not 403) to avoid leaking existence.
        var isOwner = listing.Provider?.UserId == userId;
        var isAdmin = currentUser.IsInRole("Admin");
        if (!isOwner && !isAdmin)
        {
            logger.LogWarning(
                "User {UserId} attempted to access listing {ListingId} owned by a different provider.",
                userId, request.ListingId);
            return null;
        }

        // Owner view: skip public visibility rule.
        // If no published snapshot exists, the mapper falls back to the live entity — this is
        // intentional so the owner sees their current (possibly unapproved) draft.
        var snapshot = snapshotBuilder.Parse(listing.PublishedSnapshot);

        return await ListingDetailMapper.BuildAsync(listing, snapshot, listingRepository, ct);
    }
}
