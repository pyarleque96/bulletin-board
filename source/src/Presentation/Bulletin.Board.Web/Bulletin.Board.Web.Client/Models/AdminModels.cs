namespace Bulletin.Board.Web.Client.Models;

// ============================================================
// Admin-only DTOs — mirror /api/v1/admin/* endpoint contracts.
// All mutations require JWT with role "Admin" (injected by
// the BFF using the JWT from the auth cookie). Field order mirrors the API.
// ============================================================

public record AdminStatsDto(
    int PendingListings,
    int PendingReviews,
    int ContactsToday,
    int TotalProviders,
    int VipProviders,
    int VerifiedProviders,
    int RegularProviders,
    int UrgentListings
);

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

public record AdminProviderDto(
    Guid Id,
    Guid UserId,
    string? FirstName,
    string? LastName,
    string Tier,
    string VerificationStatus,
    string WhatsAppNumber,
    string? Bio,
    int Hierarchy,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    int ListingsCount,
    bool IsVipNow,
    DateTimeOffset? VipUntil,
    bool VipIsIndefinite,
    bool PhoneVerified,
    bool IdVerified,
    bool SocialVerified
);

// ── VIP toggle ───────────────────────────────────────────────

public record GrantVipRequest(DateTimeOffset? Until, bool Indefinite, string? Reason);

public record RevokeVipRequest(string? Reason);

public record ReauthRequest(string Password);

public record ReauthResponse(string ReauthToken, DateTimeOffset ExpiresAt);

public record VipChangeLogDto(
    Guid Id,
    string EntityType,
    Guid EntityId,
    string Action,
    DateTimeOffset? PreviousVipUntil,
    bool PreviousIsIndefinite,
    DateTimeOffset? NewVipUntil,
    bool NewIsIndefinite,
    Guid ChangedByUserId,
    DateTimeOffset ChangedAt,
    string? IpAddress,
    string? Reason);

public enum VipEntityKind
{
    Provider,
    Listing
}

// ── Request bodies ───────────────────────────────────────────

public record ApproveListingRequest(string? FeedbackEn, string? FeedbackEs);

public record RejectListingRequest(string Reason);

public record RequestChangesRequest(string Feedback);

public record ChangeProviderTierRequest(string Tier);

public record SetProviderHierarchyRequest(int Hierarchy);

public record RecomputeTrendingResponse(int Affected, long DurationMs);
