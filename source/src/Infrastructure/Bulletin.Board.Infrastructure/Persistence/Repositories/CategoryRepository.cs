using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Bulletin.Board.Infrastructure.Persistence.Repositories;

public sealed class CategoryRepository(ApplicationDbContext context)
    : Repository<Category>(context), ICategoryRepository
{
    public async Task<IReadOnlyList<(Category Category, int ApprovedListingCount)>> GetWithListingCountsAsync(
        CancellationToken ct = default)
    {
        var results = await DbSet
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new
            {
                Category = c,
                Count = c.Listings.Count(l => l.Status == ListingStatus.Approved)
            })
            .AsNoTracking()
            .ToListAsync(ct);

        return results
            .Select(r => (r.Category, r.Count))
            .ToArray();
    }
}
