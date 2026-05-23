using Bulletin.Board.Domain.Entities;

namespace Bulletin.Board.Domain.Interfaces.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    /// <summary>
    /// Returns active categories ordered by DisplayOrder, each annotated with
    /// the count of listings in <c>Approved</c> status.
    /// </summary>
    Task<IReadOnlyList<(Category Category, int ApprovedListingCount)>> GetWithListingCountsAsync(
        CancellationToken ct = default);
}
