using Bulletin.Board.Application.Commands.Listings;
using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using MediatR;

namespace Bulletin.Board.Application.Queries.Manage;

public record GetListingOverviewQuery(Guid ListingId) : IRequest<ListingOverviewDto?>;

public sealed class GetListingOverviewQueryHandler(
    IListingRepository listings,
    ICurrentUserService currentUser,
    IListingOverviewMetricsProvider metrics)
    : IRequestHandler<GetListingOverviewQuery, ListingOverviewDto?>
{
    public async Task<ListingOverviewDto?> Handle(GetListingOverviewQuery request, CancellationToken ct)
    {
        var listing = await ListingOwnerAuthorization.AuthorizeAsync(listings, currentUser, request.ListingId, ct);

        var (totalReviews, pendingReviews, avgRating) = await metrics.GetReviewMetricsAsync(listing.Id, ct);
        var totalContacts30d = await metrics.GetContactsLast30dAsync(listing.Id, ct);

        return new ListingOverviewDto(
            ListingId: listing.Id,
            TitleEn: listing.TitleEn,
            TitleEs: listing.TitleEs,
            CategorySlug: listing.Category?.Slug ?? string.Empty,
            Status: listing.Status.ToString(),
            Tier: listing.ProviderTier.ToString(),
            Hierarchy: listing.ProviderHierarchy,
            ApprovedAt: listing.PublishedAt,
            CreatedAt: listing.CreatedAt,
            ProviderName: listing.Provider?.WhatsAppNumber ?? string.Empty,
            ProviderPhone: listing.Provider?.WhatsAppNumber ?? string.Empty,
            ProviderEmail: string.Empty,
            AvgRating: avgRating,
            TotalReviews: totalReviews,
            PendingReviewsCount: pendingReviews,
            TotalContactsLast30d: totalContacts30d,
            IsActive: !listing.IsPausedByOwner && listing.Status == ListingStatus.Approved,
            IsPausedByOwner: listing.IsPausedByOwner,
            GmFeedback: listing.RejectionReason,
            NeedsChangesFeedback: listing.Status == ListingStatus.NeedsChanges ? listing.RejectionReason : null,
            Kpis: null);
    }
}

/// <summary>
/// Read-side helper that aggregates lightweight metrics for the manage Overview tab.
/// Lives in Application but is implemented by Infrastructure (raw EF queries) to
/// avoid pulling navigation properties for trivial counts.
/// </summary>
public interface IListingOverviewMetricsProvider
{
    Task<(int TotalReviews, int PendingReviews, double AvgRating)> GetReviewMetricsAsync(
        Guid listingId, CancellationToken ct);

    Task<int> GetContactsLast30dAsync(Guid listingId, CancellationToken ct);
}
