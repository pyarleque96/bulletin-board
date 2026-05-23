using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Bulletin.Board.Infrastructure.Persistence.Repositories;

public sealed class AdminStatsRepository(ApplicationDbContext context) : IAdminStatsRepository
{
    public Task<int> CountPendingListingsAsync(CancellationToken ct = default)
        => context.Listings
            .Where(l => !l.IsDeleted && l.Status == ListingStatus.Pending)
            .CountAsync(ct);

    public Task<int> CountPendingRatingsAsync(CancellationToken ct = default)
        => context.Ratings
            .Where(r => r.Status == RatingStatus.Pending)
            .CountAsync(ct);

    public Task<int> CountContactsTodayAsync(CancellationToken ct = default)
    {
        var todayUtc = new DateTimeOffset(DateTimeOffset.UtcNow.Date, TimeSpan.Zero);
        return context.Contacts
            .Where(c => c.CreatedAt >= todayUtc)
            .CountAsync(ct);
    }

    public Task<int> CountTotalProvidersAsync(CancellationToken ct = default)
        => context.Providers.CountAsync(ct);

    public Task<int> CountVipProvidersAsync(CancellationToken ct = default)
        => context.Providers
            .Where(p => p.Tier == ProviderTier.VIP)
            .CountAsync(ct);

    public Task<int> CountVerifiedProvidersAsync(CancellationToken ct = default)
        => context.Providers
            .Where(p => p.Tier == ProviderTier.Verified)
            .CountAsync(ct);

    public Task<int> CountRegularProvidersAsync(CancellationToken ct = default)
        => context.Providers
            .Where(p => p.Tier == ProviderTier.Regular)
            .CountAsync(ct);

    public Task<int> CountUrgentListingsAsync(CancellationToken ct = default)
    {
        var threshold = DateTimeOffset.UtcNow.AddHours(-48);
        return context.Listings
            .Where(l => !l.IsDeleted && l.Status == ListingStatus.Pending && l.CreatedAt <= threshold)
            .CountAsync(ct);
    }
}
