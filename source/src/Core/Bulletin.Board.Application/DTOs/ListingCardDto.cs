namespace Bulletin.Board.Application.DTOs;

public record ListingCardDto(
    Guid Id,
    string TitleEn,
    string TitleEs,
    string Tier,
    decimal Rating,
    int ReviewCount,
    string Location,
    string CategorySlug,
    string? CoverPhotoUrl,
    string DescriptionEn,
    string DescriptionEs,
    string[] Languages,
    string WhatsAppNumber,
    decimal? DailyRate,
    decimal? WeeklyRate,
    decimal? MonthlyRate,
    bool PhoneVerified,
    bool IdVerified,
    bool SocialVerified,
    int ProviderHierarchy
);
