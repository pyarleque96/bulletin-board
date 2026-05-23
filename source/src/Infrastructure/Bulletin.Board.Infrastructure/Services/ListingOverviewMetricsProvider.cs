using Bulletin.Board.Application.Queries.Manage;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bulletin.Board.Infrastructure.Services;

public sealed class ListingOverviewMetricsProvider(ApplicationDbContext db) : IListingOverviewMetricsProvider
{
    public async Task<(int TotalReviews, int PendingReviews, double AvgRating)> GetReviewMetricsAsync(
        Guid listingId, CancellationToken ct)
    {
        var grouped = await db.Ratings
            .AsNoTracking()
            .Where(r => r.ListingId == listingId)
            .GroupBy(r => r.Status)
            .Select(g => new { Status = g.Key, Count = g.Count(), Avg = g.Average(r => (double?)r.Stars) ?? 0d })
            .ToListAsync(ct);

        var total = grouped.Where(g => g.Status == RatingStatus.Approved).Sum(g => g.Count);
        var pending = grouped.Where(g => g.Status == RatingStatus.Pending).Sum(g => g.Count);
        var avg = grouped.FirstOrDefault(g => g.Status == RatingStatus.Approved)?.Avg ?? 0d;

        return (total, pending, Math.Round(avg, 2));
    }

    public async Task<int> GetContactsLast30dAsync(Guid listingId, CancellationToken ct)
    {
        var cutoff = DateTimeOffset.UtcNow.AddDays(-30);
        return await db.Contacts
            .AsNoTracking()
            .Where(c => c.ListingId == listingId && c.CreatedAt >= cutoff)
            .CountAsync(ct);
    }
}
