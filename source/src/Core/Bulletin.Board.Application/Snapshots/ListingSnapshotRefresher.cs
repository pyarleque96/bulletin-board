using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Snapshots;

internal sealed class ListingSnapshotRefresher(
    IListingRepository listings,
    IListingSnapshotBuilder builder,
    ILogger<ListingSnapshotRefresher> logger)
    : IListingSnapshotRefresher
{
    public async Task<bool> RefreshIfApprovedAsync(Guid listingId, CancellationToken ct = default)
    {
        var listing = await listings.GetForSnapshotAsync(listingId, ct);
        if (listing is null || listing.IsDeleted || listing.Status != ListingStatus.Approved)
            return false;

        var json = builder.Build(listing);
        listing.RefreshSnapshot(json, ListingSnapshot.CurrentVersion);
        listings.Update(listing);
        await listings.SaveChangesAsync(ct);

        logger.LogInformation(
            "Refreshed published snapshot for Approved listing {ListingId}.", listingId);
        return true;
    }

    public async Task<int> BackfillMissingAsync(CancellationToken ct = default)
    {
        var ids = await listings.GetIdsMissingSnapshotAsync(ct);
        if (ids.Count == 0) return 0;

        var refreshed = 0;
        foreach (var id in ids)
        {
            var listing = await listings.GetForSnapshotAsync(id, ct);
            if (listing is null || listing.Status != ListingStatus.Approved) continue;

            var json = builder.Build(listing);
            listing.RefreshSnapshot(json, ListingSnapshot.CurrentVersion);
            listings.Update(listing);
            refreshed++;
        }

        if (refreshed > 0)
            await listings.SaveChangesAsync(ct);

        logger.LogInformation(
            "Snapshot backfill complete: {Count} of {Total} listings updated.",
            refreshed, ids.Count);
        return refreshed;
    }
}
