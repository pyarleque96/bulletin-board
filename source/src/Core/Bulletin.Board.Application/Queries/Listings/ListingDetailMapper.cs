using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Application.Snapshots;
using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Interfaces.Repositories;

namespace Bulletin.Board.Application.Queries.Listings;

/// <summary>
/// Builds a public <see cref="ListingDetailDto"/>. Editable content (title, descriptions,
/// images, vehicles, photos) projects from the published <see cref="ListingSnapshot"/> when
/// supplied; if no snapshot exists, it falls back to the live entity (typical for seeded data
/// or never-approved listings being viewed by admins). Reviews, ratings, similar listings and
/// provider verification are always read live regardless of snapshot presence.
/// </summary>
internal static class ListingDetailMapper
{
    internal static async Task<ListingDetailDto> BuildAsync(
        Listing listing,
        ListingSnapshot? snapshot,
        IListingRepository listingRepository,
        CancellationToken ct)
    {
        var publicPhotos = BuildPhotos(listing, snapshot);
        var vehicles = BuildVehicles(listing, snapshot);

        var approvedRatings = listing.Ratings
            .Where(r => r.Status == RatingStatus.Approved)
            .OrderByDescending(r => r.CreatedAt)
            .ToList();

        var reviews = approvedRatings
            .Take(10)
            .Select(r => new ReviewDto("Anonymous", r.Stars, r.Comment ?? string.Empty, r.CreatedAt))
            .ToArray();

        var distribution = new RatingDistributionDto(
            FiveStar: approvedRatings.Count(r => r.Stars == 5),
            FourStar: approvedRatings.Count(r => r.Stars == 4),
            ThreeStar: approvedRatings.Count(r => r.Stars == 3),
            TwoStar: approvedRatings.Count(r => r.Stars == 2),
            OneStar: approvedRatings.Count(r => r.Stars == 1)
        );

        var similar = await listingRepository.GetRankedAsync(
            categoryId: listing.CategoryId,
            status: ListingStatus.Approved,
            page: 1,
            pageSize: 5,
            ct);

        var similarDtos = similar
            .Where(l => l.Id != listing.Id)
            .Take(3)
            .Select(ListingCardMapper.ToCardDto)
            .ToArray();

        var isVerified = listing.Provider?.VerificationStatus == VerificationStatus.Approved;

        // Title/description/price/etc. come from snapshot when present so a listing under
        // re-review keeps showing the GM-approved copy while the provider edits the live row.
        var titleEn        = snapshot?.TitleEn        ?? listing.TitleEn;
        var titleEs        = snapshot?.TitleEs        ?? listing.TitleEs;
        var descriptionEn  = snapshot?.DescriptionEn  ?? listing.DescriptionEn ?? string.Empty;
        var descriptionEs  = snapshot?.DescriptionEs  ?? listing.DescriptionEs ?? string.Empty;
        var location       = snapshot?.Location       ?? listing.Location       ?? string.Empty;
        var whatsApp       = snapshot?.WhatsAppNumber ?? listing.WhatsAppNumber
                             ?? listing.Provider?.WhatsAppNumber
                             ?? string.Empty;

        return new ListingDetailDto(
            Id: listing.Id,
            TitleEn: titleEn,
            TitleEs: titleEs,
            DescriptionEn: descriptionEn,
            DescriptionEs: descriptionEs,
            Tier: listing.ProviderTier.ToString(),
            Status: listing.Status.ToString(),
            GmNote: listing.RejectionReason,
            Rating: listing.AvgRating ?? 0m,
            ReviewCount: approvedRatings.Count,
            Location: location,
            CategorySlug: listing.Category?.Slug ?? string.Empty,
            WhatsAppNumber: whatsApp,
            DailyRate: null,
            WeeklyRate: null,
            MonthlyRate: null,
            PhoneVerified: isVerified,
            IdVerified: isVerified,
            SocialVerified: isVerified,
            ApprovedAt: listing.PublishedAt,
            Languages: ["en", "es"],
            Photos: publicPhotos,
            Services: [],
            Vehicles: vehicles,
            Reviews: reviews,
            RatingDistribution: distribution,
            SimilarListings: similarDtos
        );
    }

    private static ListingPhotoDto[] BuildPhotos(Listing listing, ListingSnapshot? snapshot)
    {
        if (snapshot is not null)
        {
            return snapshot.Images
                .Select(i => new ListingPhotoDto(i.Id, i.FilePath, snapshot.TitleEn, i.DisplayOrder))
                .ToArray();
        }

        return listing.Images
            .Where(i => i.IsPublic)
            .OrderBy(i => i.DisplayOrder)
            .Select(i => new ListingPhotoDto(i.Id, i.FilePath, listing.TitleEn, i.DisplayOrder))
            .ToArray();
    }

    private static VehicleDto[] BuildVehicles(Listing listing, ListingSnapshot? snapshot)
    {
        if (snapshot is not null)
        {
            return snapshot.Vehicles
                .Select(v => new VehicleDto(
                    Id: v.Id,
                    Name: v.Name,
                    PassengerMax: v.PassengerMax,
                    Transmission: v.Transmission,
                    HasAirConditioning: v.HasAirConditioning,
                    DailyRateCents: v.DailyRateCents,
                    WeeklyRateCents: v.WeeklyRateCents,
                    MonthlyRateCents: v.MonthlyRateCents,
                    PrimaryPhotoUrl: v.Photos
                        .OrderByDescending(p => p.IsPrimary)
                        .ThenBy(p => p.DisplayOrder)
                        .Select(p => p.FilePath)
                        .FirstOrDefault(),
                    DisplayOrder: v.DisplayOrder))
                .ToArray();
        }

        return listing.Vehicles
            .Where(v => v.IsActive)
            .OrderBy(v => v.DisplayOrder)
            .ThenBy(v => v.CreatedAt)
            .Select(v => new VehicleDto(
                Id: v.Id,
                Name: v.Name,
                PassengerMax: v.PassengerMax,
                Transmission: v.Transmission.ToString(),
                HasAirConditioning: v.HasAirConditioning,
                DailyRateCents: v.DailyRateCents,
                WeeklyRateCents: v.WeeklyRateCents,
                MonthlyRateCents: v.MonthlyRateCents,
                PrimaryPhotoUrl: v.Photos
                    .Where(p => p.IsPublic)
                    .OrderByDescending(p => p.IsPrimary)
                    .ThenBy(p => p.DisplayOrder)
                    .Select(p => p.FilePath)
                    .FirstOrDefault(),
                DisplayOrder: v.DisplayOrder))
            .ToArray();
    }
}
