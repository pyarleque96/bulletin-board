using Bulletin.Board.Application.Commands.Listings;
using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Queries.Listings;

public sealed class GetListingAnalyticsQueryHandler(
    IListingRepository listings,
    IRepository<Domain.Entities.Contact> contacts,
    IRepository<Domain.Entities.Rating> ratings,
    IRepository<ListingView> views,
    ICurrentUserService currentUser,
    ILogger<GetListingAnalyticsQueryHandler> logger)
    : IRequestHandler<GetListingAnalyticsQuery, ListingAnalyticsDto>
{
    public async Task<ListingAnalyticsDto> Handle(GetListingAnalyticsQuery request, CancellationToken ct)
    {
        await ListingOwnerAuthorization.AuthorizeAsync(listings, currentUser, request.ListingId, ct);

        // Contacts for this listing
        var listingContacts = await contacts.FindAsync(c => c.ListingId == request.ListingId, ct);
        var contactsTotal = listingContacts.Count;
        // All contacts created via this platform default channel = WhatsApp
        var whatsAppClicks = listingContacts.Count(c =>
            string.Equals(c.Channel, "WhatsApp", StringComparison.OrdinalIgnoreCase));

        // Ratings for this listing (approved only for public KPIs)
        var listingRatings = await ratings.FindAsync(
            r => r.ListingId == request.ListingId && r.Status == RatingStatus.Approved, ct);
        var totalRatings = listingRatings.Count;
        var avgRating = totalRatings > 0
            ? Math.Round((decimal)listingRatings.Average(r => r.Stars), 2)
            : 0m;

        var now = DateTimeOffset.UtcNow;
        var periodStart = now.AddDays(-30);
        var prevPeriodStart = now.AddDays(-60);
        var todayStart = now.Date;
        var weekStart = now.AddDays(-7);

        // Views — sourced from listing_views populated by POST /api/v1/listings/{id}/view
        var allViews = await views.FindAsync(v => v.ListingId == request.ListingId, ct);
        var viewsTotal = allViews.Count;
        var viewsToday = allViews.Count(v => v.ViewedAt >= todayStart);
        var viewsThisWeek = allViews.Count(v => v.ViewedAt >= weekStart);
        var viewsThisMonth = allViews.Count(v => v.ViewedAt >= periodStart);
        var viewsPrevMonth = allViews.Count(v => v.ViewedAt >= prevPeriodStart && v.ViewedAt < periodStart);

        // Daily breakdown for the last 30 days. Zero-fill missing dates so the chart
        // shows a continuous line even when some days had no traffic.
        var dailyViews = Enumerable.Range(0, 30)
            .Select(i => DateOnly.FromDateTime(now.AddDays(-29 + i).UtcDateTime))
            .Select(date => new DailyViewDto(
                date,
                allViews.Count(v => DateOnly.FromDateTime(v.ViewedAt.UtcDateTime) == date)))
            .ToArray();

        int? viewsMonthDeltaPercent = viewsPrevMonth > 0
            ? (viewsThisMonth - viewsPrevMonth) * 100 / viewsPrevMonth
            : null;

        var whatsAppCurrent = listingContacts.Count(c =>
            string.Equals(c.Channel, "WhatsApp", StringComparison.OrdinalIgnoreCase)
            && c.CreatedAt >= periodStart);
        var whatsAppPrevious = listingContacts.Count(c =>
            string.Equals(c.Channel, "WhatsApp", StringComparison.OrdinalIgnoreCase)
            && c.CreatedAt >= prevPeriodStart && c.CreatedAt < periodStart);

        int? whatsAppContactsDeltaPercent = whatsAppPrevious > 0
            ? (whatsAppCurrent - whatsAppPrevious) * 100 / whatsAppPrevious
            : null;

        logger.LogInformation(
            "Analytics for listing {ListingId}: contacts={Contacts}, ratings={Ratings}.",
            request.ListingId, contactsTotal, totalRatings);

        return new ListingAnalyticsDto(
            ViewsTotal: viewsTotal,
            ViewsToday: viewsToday,
            ViewsThisWeek: viewsThisWeek,
            ViewsThisMonth: viewsThisMonth,
            DailyViews: dailyViews,
            ContactsTotal: contactsTotal,
            WhatsAppClicksTotal: whatsAppClicks,
            AvgRating: avgRating,
            TotalRatings: totalRatings,
            ViewsMonthDeltaPercent: viewsMonthDeltaPercent,
            WhatsAppContactsDeltaPercent: whatsAppContactsDeltaPercent);
    }
}
