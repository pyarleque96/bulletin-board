namespace Bulletin.Board.Application.DTOs.Admin;

public record AdminListingDetailDto(
    Guid Id,
    string TitleEn,
    string TitleEs,
    string? DescriptionEn,
    string? DescriptionEs,
    decimal? Price,
    string? Location,
    string? WhatsAppNumber,
    string Status,
    string? RejectionReason,
    string ProviderTier,
    decimal? AvgRating,
    bool IsDeleted,
    DateTimeOffset CreatedAt,
    DateTimeOffset? PublishedAt,
    Guid ProviderId,
    string ProviderName,
    Guid CategoryId,
    string CategoryName,
    IReadOnlyList<AdminImageDto> Images);
