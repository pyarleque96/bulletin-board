namespace Bulletin.Board.Application.DTOs;

/// <summary>
/// Aggregated overview for the /manage [id]/overview tab. KPIs categoría-específicos
/// se encapsulan en <see cref="Kpis"/> como objeto polimórfico — cada categoría puede
/// agregarse sin breaking change al contrato base.
/// </summary>
public record ListingOverviewDto(
    Guid ListingId,
    string TitleEn,
    string TitleEs,
    string CategorySlug,
    string Status,
    string Tier,
    int Hierarchy,
    DateTimeOffset? ApprovedAt,
    DateTimeOffset CreatedAt,
    string ProviderName,
    string ProviderPhone,
    string ProviderEmail,
    double AvgRating,
    int TotalReviews,
    int PendingReviewsCount,
    int TotalContactsLast30d,
    bool IsActive,
    bool IsPausedByOwner,
    string? GmFeedback,
    string? NeedsChangesFeedback,
    object? Kpis);
