using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Bulletin.Board.Infrastructure.Persistence.Repositories;

public sealed class AdminRatingsRepository(ApplicationDbContext context) : IAdminRatingsRepository
{
    public async Task<(IReadOnlyList<AdminRatingProjection> Items, int Total)> GetPagedAsync(
        string? status,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = context.Ratings
            .Include(r => r.Listing)
            .Include(r => r.Provider)
            .Include(r => r.Contact)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<RatingStatus>(status, ignoreCase: true, out var statusFilter))
            query = query.Where(r => r.Status == statusFilter);

        var total = await query.CountAsync(ct);

        var rawItems = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new
            {
                r.Id,
                ListingTitleEn = r.Listing != null ? r.Listing.TitleEn : r.ListingId.ToString(),
                ProviderWhatsApp = r.Provider != null ? r.Provider.WhatsAppNumber : r.ProviderId.ToString(),
                r.AuthorUserId,
                r.Stars,
                r.Comment,
                Status = r.Status.ToString(),
                r.CreatedAt,
                ContactDate = r.Contact != null ? (DateTimeOffset?)r.Contact.CreatedAt : null
            })
            .ToListAsync(ct);

        var userIds = rawItems.Select(x => x.AuthorUserId).Distinct().ToList();

        var userMap = await context.Users
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.UserName, u.Email })
            .ToDictionaryAsync(u => u.Id, ct);

        var projections = rawItems.Select(r =>
        {
            userMap.TryGetValue(r.AuthorUserId, out var user);
            return new AdminRatingProjection(
                Id: r.Id,
                ListingTitleEn: r.ListingTitleEn,
                ProviderWhatsApp: r.ProviderWhatsApp,
                ReviewerName: user?.UserName ?? r.AuthorUserId.ToString(),
                ReviewerEmail: user?.Email,
                Stars: r.Stars,
                Comment: r.Comment,
                Status: r.Status,
                CreatedAt: r.CreatedAt,
                ContactDate: r.ContactDate);
        }).ToList();

        return (projections, total);
    }
}
