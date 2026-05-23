namespace Bulletin.Board.Domain.Interfaces.Repositories;

public record AdminRatingProjection(
    Guid Id,
    string ListingTitleEn,
    string ProviderWhatsApp,
    string ReviewerName,
    string? ReviewerEmail,
    short Stars,
    string? Comment,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ContactDate
);

public interface IAdminRatingsRepository
{
    Task<(IReadOnlyList<AdminRatingProjection> Items, int Total)> GetPagedAsync(
        string? status,
        int page,
        int pageSize,
        CancellationToken ct = default);
}
