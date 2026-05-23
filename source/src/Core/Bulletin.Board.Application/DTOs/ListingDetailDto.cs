namespace Bulletin.Board.Application.DTOs;

public record ListingDetailDto(
    Guid Id,
    string TitleEn,
    string TitleEs,
    string DescriptionEn,
    string DescriptionEs,
    string Tier,
    string Status,
    string? GmNote,
    decimal Rating,
    int ReviewCount,
    string Location,
    string CategorySlug,
    string WhatsAppNumber,
    decimal? DailyRate,
    decimal? WeeklyRate,
    decimal? MonthlyRate,
    bool PhoneVerified,
    bool IdVerified,
    bool SocialVerified,
    DateTimeOffset? ApprovedAt,
    string[] Languages,
    ListingPhotoDto[] Photos,
    ServiceItemDto[] Services,
    VehicleDto[] Vehicles,
    ReviewDto[] Reviews,
    RatingDistributionDto RatingDistribution,
    ListingCardDto[] SimilarListings
);

public record ListingPhotoDto(Guid Id, string Url, string AltText, int Order);

public record ServiceItemDto(
    string NameEn,
    string NameEs,
    string? DescriptionEn,
    string? DescriptionEs,
    string? Price,
    string GroupEn,
    string GroupEs
);

public record VehicleDto(
    Guid Id,
    string Name,
    int PassengerMax,
    string Transmission,         // "Manual" | "Automatic" | "Both"
    bool HasAirConditioning,
    int DailyRateCents,
    int? WeeklyRateCents,
    int? MonthlyRateCents,
    string? PrimaryPhotoUrl,
    int DisplayOrder
);

public record ReviewDto(string ReviewerName, decimal Rating, string Text, DateTimeOffset CreatedAt);

public record RatingDistributionDto(int FiveStar, int FourStar, int ThreeStar, int TwoStar, int OneStar);
