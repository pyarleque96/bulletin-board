namespace Bulletin.Board.Application.Snapshots;

/// <summary>
/// Refreshes the published snapshot of a listing that is currently in <c>Approved</c> state.
/// Used by GM actions that change what public reads see without going through the normal
/// approve flow (e.g. flipping a photo's visibility).
/// </summary>
/// <remarks>
/// If the listing is NOT Approved (Pending / Rejected / NeedsChanges), the call is a no-op:
/// the next Approve will capture a fresh snapshot anyway, and we don't want a half-edited
/// draft escaping into the public view.
/// </remarks>
public interface IListingSnapshotRefresher
{
    /// <summary>Regenerates and persists the snapshot if the listing is Approved.</summary>
    /// <returns>true when the snapshot was rewritten; false when the call was a no-op.</returns>
    Task<bool> RefreshIfApprovedAsync(Guid listingId, CancellationToken ct = default);

    /// <summary>
    /// One-shot data fix: finds every Approved listing without a snapshot and generates one.
    /// Used at startup to migrate seeded/legacy listings into the snapshot model.
    /// </summary>
    /// <returns>The number of listings backfilled.</returns>
    Task<int> BackfillMissingAsync(CancellationToken ct = default);
}
