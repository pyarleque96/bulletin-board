using Bulletin.Board.Domain.Entities;

namespace Bulletin.Board.Application.Snapshots;

public interface IListingSnapshotBuilder
{
    /// <summary>
    /// Builds the canonical published snapshot JSON for a listing. The caller is
    /// responsible for loading the full aggregate beforehand (Images public-only,
    /// Vehicles active-only with public photos). The builder does NOT touch the DB.
    /// </summary>
    string Build(Listing listing);

    /// <summary>
    /// Inverse of <see cref="Build"/> — parses persisted JSON back into the typed snapshot.
    /// Returns null when the stored payload is unparseable (logged elsewhere).
    /// </summary>
    ListingSnapshot? Parse(string? snapshotJson);
}
