namespace Bulletin.Board.Domain.Interfaces.Repositories;

public interface IAdminStatsRepository
{
    Task<int> CountPendingListingsAsync(CancellationToken ct = default);
    Task<int> CountPendingRatingsAsync(CancellationToken ct = default);
    Task<int> CountContactsTodayAsync(CancellationToken ct = default);
    Task<int> CountTotalProvidersAsync(CancellationToken ct = default);
    Task<int> CountVipProvidersAsync(CancellationToken ct = default);
    Task<int> CountVerifiedProvidersAsync(CancellationToken ct = default);
    Task<int> CountRegularProvidersAsync(CancellationToken ct = default);

    /// <summary>
    /// Listings in Pending status waiting more than 48 hours.
    /// </summary>
    Task<int> CountUrgentListingsAsync(CancellationToken ct = default);
}
