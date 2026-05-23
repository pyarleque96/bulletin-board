using Bulletin.Board.Domain.Interfaces.Services;
using Bulletin.Board.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bulletin.Board.Infrastructure.Services;

/// <summary>
/// Single source of truth for the trending-score SQL. Consumed by both the hourly
/// background recalculator and the admin recompute endpoint.
///
/// Formula (CLAUDE.md §3):
///   trending_score =
///       provider_hierarchy * 1000           -- GM editorial boost dominates
///     + tier_weight        * 50             -- VIP=3 / Verified=2 / Regular=1
///     + AvgRating          * 20             -- 0..5 stars
///     + ln(1 + whatsapp_contacts_30d) * 30  -- logarithmic to prevent runaway concentration
///     + ln(1 + views_30d)             * 10  -- weaker than contacts (view ≠ intent)
///     + recency_boost                       -- 30 * exp(-days_since_published / 60)
/// </summary>
public sealed class TrendingScoreCalculator(ApplicationDbContext db) : ITrendingScoreCalculator
{
    private const string RecalcSql = """
        UPDATE listings AS l
        SET trending_score = (
                l.provider_hierarchy * 1000.0
              + CASE l."ProviderTier"
                    WHEN 'VIP'      THEN 150.0
                    WHEN 'Verified' THEN 100.0
                    ELSE                 50.0
                END
              + COALESCE(l."AvgRating", 0)::double precision * 20.0
              + LN(1 + (
                    SELECT COUNT(*)::double precision
                    FROM contact_logs cl
                    WHERE cl."ListingId" = l."Id"
                      AND cl."ContactedAt" >= NOW() - INTERVAL '30 days'
                )) * 30.0
              + LN(1 + (
                    SELECT COUNT(*)::double precision
                    FROM listing_views lv
                    WHERE lv.listing_id = l."Id"
                      AND lv.viewed_at >= NOW() - INTERVAL '30 days'
                )) * 10.0
              + 30.0 * EXP(
                    -EXTRACT(EPOCH FROM (NOW() - COALESCE(l."PublishedAt", l."CreatedAt")))
                    / 5184000.0
                )
            ),
            score_updated_at = NOW()
        WHERE l."IsDeleted" = false
          AND l."Status" = 'Approved'
          AND l.is_paused_by_owner = false;
        """;

    public Task<int> RecalculateAsync(CancellationToken ct = default)
        => db.Database.ExecuteSqlRawAsync(RecalcSql, ct);
}
