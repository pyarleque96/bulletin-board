namespace Bulletin.Board.Application.DTOs.Admin;

public record AdminListingDto(
    Guid Id,
    string TitleEn,
    string TitleEs,
    string ProviderName,
    string? ProviderPhone,
    string ProviderTier,
    string CategoryName,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    decimal? AvgRating,
    int PhotoCount,
    bool IsVipNow,
    DateTimeOffset? VipUntil,
    bool VipIsIndefinite
);
