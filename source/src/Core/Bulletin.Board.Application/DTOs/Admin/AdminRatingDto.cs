namespace Bulletin.Board.Application.DTOs.Admin;

public record AdminRatingDto(
    Guid Id,
    string ListingTitleEn,
    string ProviderName,
    string ReviewerName,
    string? ReviewerEmail,
    short Stars,
    string? Comment,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ContactDate
);
