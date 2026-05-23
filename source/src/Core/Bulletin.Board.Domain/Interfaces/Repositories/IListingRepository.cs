using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;

namespace Bulletin.Board.Domain.Interfaces.Repositories;

public interface IListingRepository : IRepository<Listing>
{
    Task<IReadOnlyList<Listing>> GetRankedAsync(Guid? categoryId, ListingStatus status,
        int page, int pageSize, CancellationToken ct = default);

    /// <summary>
    /// Returns Approved, non-deleted, non-paused listings ordered by <c>trending_score DESC</c>.
    /// Score is recomputed hourly by <c>TrendingScoreRecalculatorService</c>.
    /// Caller is expected to over-fetch and apply diversity caps (e.g. max-N per provider).
    /// </summary>
    Task<IReadOnlyList<Listing>> GetTrendingAsync(int take, CancellationToken ct = default);

    Task<IReadOnlyList<Listing>> GetPendingAsync(int page, int pageSize, CancellationToken ct = default);

    /// <summary>
    /// Loads a listing with all navigations needed for the detail page:
    /// Provider, Category, public Images, and approved Ratings.
    /// </summary>
    Task<Listing?> GetDetailAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Same as <see cref="GetDetailAsync"/> but keyed by the public slug.
    /// Slugs are stored lowercase and normalized; the lookup is case-sensitive
    /// because all writes pass through SlugGenerator.Slugify.
    /// </summary>
    Task<Listing?> GetDetailBySlugAsync(string slug, CancellationToken ct = default);

    /// <summary>
    /// Loads a listing with all navigations for admin review:
    /// Provider, Category, ALL Images (public and private), and ALL Ratings.
    /// </summary>
    Task<Listing?> GetAdminDetailAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Loads a listing with everything needed to build a published snapshot:
    /// public Images, active Vehicles + their public Photos. No ratings (snapshot is content-only).
    /// </summary>
    Task<Listing?> GetForSnapshotAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Returns IDs of Approved, non-deleted listings whose <c>PublishedSnapshot</c> is null.
    /// Used by the snapshot backfill at startup.
    /// </summary>
    Task<IReadOnlyList<Guid>> GetIdsMissingSnapshotAsync(CancellationToken ct = default);

    /// <summary>
    /// All non-deleted listings owned by the user (via Provider.UserId). Used by /account/listings.
    /// Includes Category for slug/icon display.
    /// </summary>
    Task<IReadOnlyList<Listing>> GetByProviderUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Paginates non-deleted listings owned by the user. Orden estable: UpdatedAt DESC, Id ASC.
    /// Returns (page items, total count).
    /// </summary>
    Task<(IReadOnlyList<Listing> Items, int Total)> GetByProviderUserIdPagedAsync(
        Guid userId, int skip, int take, CancellationToken ct = default);

    /// <summary>
    /// Returns listings for the admin panel with Provider and Category loaded.
    /// Supports optional filtering by Status, Tier, CategoryId, and ProviderId
    /// (este último alimenta la acción "View listings" desde /admin/providers).
    /// </summary>
    Task<(IReadOnlyList<Listing> Items, int Total)> GetAdminListingsPagedAsync(
        ListingStatus? status,
        ProviderTier? tier,
        Guid? categoryId,
        Guid? providerId,
        int page,
        int pageSize,
        CancellationToken ct = default);

    /// <summary>
    /// Returns a map of UserId → display name ("FirstName LastName" or email fallback)
    /// for the given set of user IDs. Used to enrich admin listing DTOs.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, string>> GetProviderDisplayNamesByUserIdsAsync(
        IEnumerable<Guid> userIds,
        CancellationToken ct = default);

    /// <summary>
    /// Cursor-paginated listings owned by the user (manage v3 infinity scroll).
    /// Canonical order: <c>ProviderHierarchy DESC, ProviderTier DESC, AvgRating DESC,
    /// CreatedAt DESC, Id DESC</c>. Filters: status, categorySlug, search.
    /// Returns one extra row past the requested page size so the caller can derive HasMore
    /// without a count query. The total is computed once on the first request (cursor null).
    /// </summary>
    Task<ManageListingsPage> GetManagePageAsync(
        Guid userId,
        ManageListingsCursorState? cursor,
        int pageSize,
        ListingStatus? status,
        string? categorySlug,
        string? search,
        bool includeTotal,
        CancellationToken ct = default);
}

/// <summary>
/// Result envelope returned by <see cref="IListingRepository.GetManagePageAsync"/>.
/// </summary>
public sealed record ManageListingsPage(
    IReadOnlyList<Listing> Items,
    bool HasMore,
    int? Total);

/// <summary>
/// State of a cursor decoded from the request — minimal subset the repository needs to
/// keyset-paginate. Kept inside Domain.Interfaces to avoid coupling to Application.
/// </summary>
public sealed record ManageListingsCursorState(
    int Hierarchy,
    int ProviderTier,
    decimal AvgRating,
    DateTimeOffset CreatedAt,
    Guid Id);
