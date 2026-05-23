namespace Bulletin.Board.Domain.Interfaces.Services;

/// <summary>
/// Recomputes <c>listings.trending_score</c> across every Approved, non-deleted, non-paused
/// listing. Owned by the background service that runs hourly, also reachable via the admin
/// recompute endpoint when the GM wants to apply a hierarchy change immediately.
/// </summary>
public interface ITrendingScoreCalculator
{
    /// <summary>
    /// Returns the number of listings whose score was updated.
    /// </summary>
    Task<int> RecalculateAsync(CancellationToken ct = default);
}
