using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Bulletin.Board.Infrastructure.Persistence.Repositories;

public sealed class ListingRepository(ApplicationDbContext context)
    : Repository<Listing>(context), IListingRepository
{
    public async Task<IReadOnlyList<Listing>> GetRankedAsync(Guid? categoryId, ListingStatus status,
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = DbSet
            .Where(l => !l.IsDeleted && !l.IsPausedByOwner && l.Status == status)
            .AsQueryable();

        if (categoryId.HasValue)
            query = query.Where(l => l.CategoryId == categoryId.Value);

        // /postgres-best-practices covering indexes: ordenar por ProviderHierarchy (columna
        // denormalizada) permite al planner usar idx_listings_ranked sin JOIN a providers.
        return await query
            .Include(l => l.Provider)
            .Include(l => l.Category)
            .Include(l => l.Images.Where(i => i.Status == PhotoStatus.Public))
            .Include(l => l.Ratings.Where(r => r.Status == RatingStatus.Approved))
            .OrderByDescending(l => l.ProviderHierarchy)
            .ThenByDescending(l => l.ProviderTier)
            .ThenByDescending(l => l.AvgRating)
            .ThenByDescending(l => l.CreatedAt)
            .ThenByDescending(l => l.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Listing>> GetTrendingAsync(int take, CancellationToken ct = default)
    {
        // Score is precomputed by TrendingScoreRecalculatorService (hourly). This query
        // exploits idx_listings_trending (partial, descending on trending_score).
        // Tie-breakers keep the order deterministic when many listings score identically
        // (typical right after a fresh deploy when scores are still 0).
        return await DbSet
            .Where(l => !l.IsDeleted
                        && !l.IsPausedByOwner
                        && l.Status == ListingStatus.Approved)
            .Include(l => l.Provider)
            .Include(l => l.Category)
            .Include(l => l.Images.Where(i => i.Status == PhotoStatus.Public))
            .Include(l => l.Ratings.Where(r => r.Status == RatingStatus.Approved))
            .OrderByDescending(l => l.TrendingScore)
            .ThenByDescending(l => l.ProviderHierarchy)
            .ThenByDescending(l => l.ProviderTier)
            .ThenByDescending(l => l.AvgRating)
            .ThenByDescending(l => l.CreatedAt)
            .ThenByDescending(l => l.Id)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Listing>> GetPendingAsync(int page, int pageSize, CancellationToken ct = default)
        => await DbSet
            .Where(l => l.Status == ListingStatus.Pending && !l.IsDeleted)
            .OrderBy(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public async Task<Listing?> GetDetailAsync(Guid id, CancellationToken ct = default)
        => await DbSet
            .Where(l => l.Id == id && !l.IsPausedByOwner)
            .Include(l => l.Provider)
            .Include(l => l.Category)
            .Include(l => l.Images.Where(i => i.Status == PhotoStatus.Public))
            .Include(l => l.Ratings.Where(r => r.Status == RatingStatus.Approved))
            .Include(l => l.Vehicles.Where(v => v.IsActive))
                .ThenInclude(v => v.Photos.Where(p => p.IsPublic))
            .FirstOrDefaultAsync(ct);

    public async Task<Listing?> GetDetailBySlugAsync(string slug, CancellationToken ct = default)
        => await DbSet
            .Where(l => l.Slug == slug && !l.IsPausedByOwner)
            .Include(l => l.Provider)
            .Include(l => l.Category)
            .Include(l => l.Images.Where(i => i.Status == PhotoStatus.Public))
            .Include(l => l.Ratings.Where(r => r.Status == RatingStatus.Approved))
            .Include(l => l.Vehicles.Where(v => v.IsActive))
                .ThenInclude(v => v.Photos.Where(p => p.IsPublic))
            .FirstOrDefaultAsync(ct);

    public async Task<Listing?> GetForSnapshotAsync(Guid id, CancellationToken ct = default)
        => await DbSet
            .Where(l => l.Id == id)
            .Include(l => l.Images.Where(i => i.Status == PhotoStatus.Public))
            .Include(l => l.Vehicles.Where(v => v.IsActive))
                .ThenInclude(v => v.Photos.Where(p => p.IsPublic))
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<Guid>> GetIdsMissingSnapshotAsync(CancellationToken ct = default)
        => await DbSet
            .Where(l => !l.IsDeleted
                        && l.Status == ListingStatus.Approved
                        && l.PublishedSnapshot == null)
            .Select(l => l.Id)
            .ToListAsync(ct);

    public async Task<Listing?> GetAdminDetailAsync(Guid id, CancellationToken ct = default)
        => await DbSet
            .Where(l => l.Id == id)
            .Include(l => l.Provider)
            .Include(l => l.Category)
            .Include(l => l.Images)
            .Include(l => l.Ratings)
            .FirstOrDefaultAsync(ct);

    public async Task<(IReadOnlyList<Listing> Items, int Total)> GetAdminListingsPagedAsync(
        ListingStatus? status,
        ProviderTier? tier,
        Guid? categoryId,
        Guid? providerId,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = DbSet
            .Where(l => !l.IsDeleted)
            .Include(l => l.Provider)
            .Include(l => l.Category)
            .Include(l => l.Images)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(l => l.Status == status.Value);

        if (tier.HasValue)
            query = query.Where(l => l.ProviderTier == tier.Value);

        if (categoryId.HasValue)
            query = query.Where(l => l.CategoryId == categoryId.Value);

        if (providerId.HasValue)
            query = query.Where(l => l.ProviderId == providerId.Value);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(l => l.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<IReadOnlyList<Listing>> GetByProviderUserIdAsync(
        Guid userId, CancellationToken ct = default)
        => await DbSet
            .Where(l => !l.IsDeleted && l.Provider.UserId == userId)
            .Include(l => l.Category)
            .Include(l => l.Provider)
            .OrderByDescending(l => l.UpdatedAt)
            .ThenBy(l => l.Id)
            .ToListAsync(ct);

    public async Task<(IReadOnlyList<Listing> Items, int Total)> GetByProviderUserIdPagedAsync(
        Guid userId, int skip, int take, CancellationToken ct = default)
    {
        var baseQuery = DbSet
            .Where(l => !l.IsDeleted && l.Provider.UserId == userId)
            .AsNoTracking();

        var total = await baseQuery.CountAsync(ct);

        var items = await baseQuery
            .Include(l => l.Category)
            .Include(l => l.Provider)
            .OrderByDescending(l => l.UpdatedAt)
            .ThenBy(l => l.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<ManageListingsPage> GetManagePageAsync(
        Guid userId,
        ManageListingsCursorState? cursor,
        int pageSize,
        ListingStatus? status,
        string? categorySlug,
        string? search,
        bool includeTotal,
        CancellationToken ct = default)
    {
        var baseQuery = DbSet
            .AsNoTracking()
            .Where(l => !l.IsDeleted && l.Provider.UserId == userId);

        if (status.HasValue)
            baseQuery = baseQuery.Where(l => l.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(categorySlug))
            baseQuery = baseQuery.Where(l => l.Category.Slug == categorySlug);

        if (!string.IsNullOrWhiteSpace(search))
        {
            // Postgres-best-practices: ILIKE evita JOIN a full-text por ahora; el listado
            // del provider raramente tiene >50 listings, así que sequential scan está bien.
            var pattern = $"%{search.Trim()}%";
            baseQuery = baseQuery.Where(l =>
                EF.Functions.ILike(l.TitleEn, pattern) ||
                EF.Functions.ILike(l.TitleEs, pattern));
        }

        // Keyset pagination — aplica WHERE compuesto que respeta el orden canónico.
        if (cursor is not null)
        {
            // (a,b,c,d,e) < (A,B,C,D,E) en orden DESC se expande explícitamente:
            //   a < A OR (a = A AND (b < B OR (b = B AND (c < C OR ... ))))
            var c = cursor;
            var avg = c.AvgRating;
            baseQuery = baseQuery.Where(l =>
                l.ProviderHierarchy < c.Hierarchy ||
                (l.ProviderHierarchy == c.Hierarchy && (
                    (int)l.ProviderTier < c.ProviderTier ||
                    ((int)l.ProviderTier == c.ProviderTier && (
                        (l.AvgRating ?? 0m) < avg ||
                        ((l.AvgRating ?? 0m) == avg && (
                            l.CreatedAt < c.CreatedAt ||
                            (l.CreatedAt == c.CreatedAt && l.Id.CompareTo(c.Id) < 0)
                        ))
                    ))
                ))
            );
        }

        int? total = null;
        if (includeTotal)
            total = await baseQuery.CountAsync(ct);

        // Fetch one extra row to derive HasMore without a second query.
        var fetchSize = pageSize + 1;
        var items = await baseQuery
            .Include(l => l.Category)
            .Include(l => l.Provider)
            .OrderByDescending(l => l.ProviderHierarchy)
            .ThenByDescending(l => l.ProviderTier)
            .ThenByDescending(l => l.AvgRating)
            .ThenByDescending(l => l.CreatedAt)
            .ThenByDescending(l => l.Id)
            .Take(fetchSize)
            .ToListAsync(ct);

        var hasMore = items.Count > pageSize;
        if (hasMore) items.RemoveAt(items.Count - 1);

        return new ManageListingsPage(items, hasMore, total);
    }

    public async Task<IReadOnlyDictionary<Guid, string>> GetProviderDisplayNamesByUserIdsAsync(
        IEnumerable<Guid> userIds,
        CancellationToken ct = default)
    {
        var ids = userIds.ToHashSet();
        var users = await context.Users
            .Where(u => ids.Contains(u.Id))
            .Select(u => new { u.Id, u.FirstName, u.LastName, u.Email })
            .ToListAsync(ct);

        return users.ToDictionary(
            u => u.Id,
            u =>
            {
                var name = $"{u.FirstName} {u.LastName}".Trim();
                return string.IsNullOrWhiteSpace(name) ? (u.Email ?? u.Id.ToString()) : name;
            });
    }
}
