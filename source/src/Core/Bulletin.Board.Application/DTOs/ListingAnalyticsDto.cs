namespace Bulletin.Board.Application.DTOs;

/// <summary>
/// KPI summary for the owner's listing dashboard.
/// Delta fields are null when the previous 30-day period has no data.
/// </summary>
public record ListingAnalyticsDto(
    // Views — sourced from listing_views via POST /api/v1/listings/{id}/view.
    int ViewsTotal,
    int ViewsToday,
    int ViewsThisWeek,
    int ViewsThisMonth,
    IReadOnlyList<DailyViewDto> DailyViews,

    // Contacts — sourced from the contacts table filtered by listing.
    int ContactsTotal,
    int WhatsAppClicksTotal,

    // Ratings — sourced live from ratings table.
    decimal AvgRating,
    int TotalRatings,

    // Deltas — % change vs the previous 30-day period. Null when previous period has no data.
    int? ViewsMonthDeltaPercent,
    int? WhatsAppContactsDeltaPercent
);

public record DailyViewDto(DateOnly Date, int Count);
