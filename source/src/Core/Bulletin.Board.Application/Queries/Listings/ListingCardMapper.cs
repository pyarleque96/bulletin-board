using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;

namespace Bulletin.Board.Application.Queries.Listings;

/// <summary>
/// Shared mapping helper for converting a Listing domain entity to ListingCardDto.
/// </summary>
internal static class ListingCardMapper
{
    internal static ListingCardDto ToCardDto(Listing l) => new(
        Id: l.Id,
        TitleEn: l.TitleEn,
        TitleEs: l.TitleEs,
        Tier: l.ProviderTier.ToString(),
        Rating: l.AvgRating ?? 0m,
        ReviewCount: l.Ratings.Count(r => r.Status == RatingStatus.Approved),
        Location: l.Location ?? string.Empty,
        CategorySlug: l.Category?.Slug ?? string.Empty,
        CoverPhotoUrl: l.Images
            .Where(i => i.IsPublic)
            .OrderBy(i => i.DisplayOrder)
            .Select(i => i.FilePath)
            .FirstOrDefault(),
        DescriptionEn: l.DescriptionEn ?? string.Empty,
        DescriptionEs: l.DescriptionEs ?? string.Empty,
        Languages: ["en", "es"],
        WhatsAppNumber: l.WhatsAppNumber ?? l.Provider?.WhatsAppNumber ?? string.Empty,
        DailyRate: null,
        WeeklyRate: null,
        MonthlyRate: null,
        PhoneVerified: l.Provider?.VerificationStatus == VerificationStatus.Approved,
        IdVerified: l.Provider?.VerificationStatus == VerificationStatus.Approved,
        SocialVerified: l.Provider?.VerificationStatus == VerificationStatus.Approved,
        ProviderHierarchy: l.Provider?.Hierarchy ?? 0
    );
}
