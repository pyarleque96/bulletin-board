using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Application.Queries.Manage;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Infrastructure.Identity;
using Bulletin.Board.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bulletin.Board.Infrastructure.Services;

public sealed class ManageReviewsReader(ApplicationDbContext db) : IManageReviewsReader
{
    public async Task<IReadOnlyList<ManageReviewDto>> GetReviewsAsync(
        Guid listingId, RatingStatus? statusFilter, CancellationToken ct)
    {
        var ratings = db.Ratings.AsNoTracking().Where(r => r.ListingId == listingId);
        if (statusFilter.HasValue)
            ratings = ratings.Where(r => r.Status == statusFilter.Value);

        var query = from r in ratings
                    join u in db.Set<ApplicationUser>().AsNoTracking() on r.AuthorUserId equals u.Id into uj
                    from u in uj.DefaultIfEmpty()
                    join rep in db.ReviewReplies.AsNoTracking() on r.Id equals rep.ReviewId into rj
                    from rep in rj.DefaultIfEmpty()
                    orderby r.CreatedAt descending
                    select new
                    {
                        r.Id,
                        AuthorName = (u != null) ? (u.FirstName + " " + u.LastName) : "Anonymous",
                        r.Stars,
                        r.Comment,
                        r.CreatedAt,
                        r.Status,
                        r.GmFeedback,
                        ReplyId = (Guid?)(rep != null ? rep.Id : (Guid?)null),
                        ReplyText = rep != null ? rep.Text : null,
                        ReplyStatus = rep != null ? rep.Status : (ReviewReplyStatus?)null,
                        ReplyCreatedAt = (DateTimeOffset?)(rep != null ? rep.CreatedAt : (DateTimeOffset?)null)
                    };

        var rows = await query.ToListAsync(ct);

        return rows.Select(r => new ManageReviewDto(
            Id: r.Id,
            ClientDisplayName: string.IsNullOrWhiteSpace(r.AuthorName?.Trim()) ? "Anonymous" : r.AuthorName!.Trim(),
            ClientAvatarUrl: null,
            Rating: r.Stars,
            Comment: r.Comment ?? string.Empty,
            CreatedAt: r.CreatedAt,
            Status: MapStatusToFrontend(r.Status),
            GmFeedback: r.GmFeedback,
            ProviderReply: r.ReplyId.HasValue
                ? new ReviewReplyDto(r.ReplyId.Value, r.ReplyText!, r.ReplyStatus!.ToString()!, r.ReplyCreatedAt!.Value)
                : null
        )).ToList();
    }

    public async Task<RatingSummaryDto> GetSummaryAsync(Guid listingId, CancellationToken ct)
    {
        var groups = await db.Ratings
            .AsNoTracking()
            .Where(r => r.ListingId == listingId && r.Status == RatingStatus.Approved)
            .GroupBy(r => r.Stars)
            .Select(g => new { Stars = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var dist = new Dictionary<int, int> { { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 } };
        foreach (var g in groups) dist[g.Stars] = g.Count;

        var total = groups.Sum(g => g.Count);
        var avg = total == 0 ? 0d : Math.Round(groups.Sum(g => (double)g.Stars * g.Count) / total, 2);

        return new RatingSummaryDto(avg, total, dist);
    }

    private static string MapStatusToFrontend(RatingStatus s) => s switch
    {
        RatingStatus.Approved => "Published",
        RatingStatus.Pending => "PendingGm",
        RatingStatus.Rejected => "Rejected",
        _ => s.ToString()
    };
}
