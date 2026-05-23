namespace Bulletin.Board.Web.Client.Models;

// ============================================================
// Shared DTOs — mirror exactly what the API returns via /api/v1/
// Field names and types must match the backend contract.
// ============================================================

/// <summary>
/// Lightweight card representation used in grid/search views.
/// Mirrors Application.DTOs.ListingCardDto.
/// </summary>
public record ListingCardDto(
    Guid     Id,
    string   TitleEn,
    string   TitleEs,
    string   Tier,             // "VIP" | "Verified" | "Regular"
    decimal  Rating,
    int      ReviewCount,
    string   Location,
    string   CategorySlug,
    string?  CoverPhotoUrl,
    string   DescriptionEn,
    string   DescriptionEs,
    string[] Languages,
    string   WhatsAppNumber,
    decimal? DailyRate,
    decimal? WeeklyRate,
    decimal? MonthlyRate,
    bool     PhoneVerified,
    bool     IdVerified,
    bool     SocialVerified,
    int      ProviderHierarchy  // GM-assigned ranking value; higher = appears first
);

/// <summary>
/// Category with display metadata.
/// Mirrors Application.DTOs.CategoryDto.
/// </summary>
public record CategoryDto(
    Guid    Id,
    string  NameEn,
    string  NameEs,
    string  Slug,
    string? Icon,
    int?    ListingCount = null
);

/// <summary>
/// Generic paged result wrapper.
/// </summary>
public record PagedResult<T>(
    T[]  Items,
    int  TotalCount,
    int  Page,
    int  PageSize
)
{
    public int  TotalPages       => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    public bool HasPreviousPage  => Page > 1;
    public bool HasNextPage      => Page < TotalPages;
}

/// <summary>
/// Full listing detail — used in the detail page.
/// Mirrors Application.DTOs.ListingDetailDto.
/// </summary>
public record ListingDetailDto(
    Guid                    Id,
    string                  TitleEn,
    string                  TitleEs,
    string                  DescriptionEn,
    string                  DescriptionEs,
    string                  Tier,           // "VIP" | "Verified" | "Regular"
    string                  Status,         // "Approved" | "Pending" | "Rejected" | "NeedsChanges"
    string?                 GmNote,
    decimal                 Rating,
    int                     ReviewCount,
    string                  Location,
    string                  CategorySlug,
    string                  WhatsAppNumber,
    decimal?                DailyRate,
    decimal?                WeeklyRate,
    decimal?                MonthlyRate,
    bool                    PhoneVerified,
    bool                    IdVerified,
    bool                    SocialVerified,
    DateTimeOffset?         ApprovedAt,
    string[]                Languages,
    ListingPhotoDto[]       Photos,
    ServiceItemDto[]        Services,
    VehicleDto[]            Vehicles,
    ReviewDto[]             Reviews,
    RatingDistributionDto   RatingDistribution,
    ListingCardDto[]        SimilarListings
);

/// <summary>
/// A GM-approved photo belonging to a listing.
/// </summary>
public record ListingPhotoDto(
    Guid   Id,
    string Url,
    string AltText,
    int    Order
);

/// <summary>
/// A service or price item within a listing (bilingual).
/// </summary>
public record ServiceItemDto(
    string  NameEn,
    string  NameEs,
    string? DescriptionEn,
    string? DescriptionEs,
    string? Price,
    string  GroupEn,
    string  GroupEs
);

/// <summary>
/// Vehicle metadata used in Car Rental listings.
/// </summary>
public record VehicleDto(
    Guid    Id,
    string  Name,
    int     PassengerMax,
    string  Transmission,
    bool    HasAirConditioning,
    int     DailyRateCents,
    int?    WeeklyRateCents,
    int?    MonthlyRateCents,
    string? PrimaryPhotoUrl,
    int     DisplayOrder
);

/// <summary>Owner-facing vehicle DTO. Mirrors Application.DTOs.OwnerVehicleDto.</summary>
public record OwnerVehicleDto(
    Guid    Id,
    string  Name,
    int     PassengerMax,
    string  Transmission,
    bool    HasAirConditioning,
    int     DailyRateCents,
    int?    WeeklyRateCents,
    int?    MonthlyRateCents,
    int     DisplayOrder,
    bool    IsActive,
    OwnerVehiclePhotoDto[] Photos);

public record OwnerVehiclePhotoDto(
    Guid    Id,
    string  FilePath,
    bool    IsPrimary,
    bool    IsPublic,
    int     DisplayOrder);

public record CreateVehicleRequest(
    string Name, int PassengerMax, string Transmission,
    bool HasAirConditioning, int DailyRateCents, int? WeeklyRateCents,
    int? MonthlyRateCents, int DisplayOrder);

public record UpdateVehicleRequest(
    string Name, int PassengerMax, string Transmission,
    bool HasAirConditioning, int DailyRateCents, int? WeeklyRateCents,
    int? MonthlyRateCents, int DisplayOrder);

public record AddVehiclePhotoRequest(string FilePath, int DisplayOrder, bool IsPrimary);

public record VehicleAvailabilityDto(
    Guid VehicleId,
    DateOnly From,
    DateOnly To,
    BlackoutRangeDto[] Blackouts);

public record BlackoutRangeDto(
    Guid Id,
    DateOnly StartDate,
    DateOnly EndDate,
    string Reason);

public record AddUnavailabilityRequest(
    DateOnly StartDate,
    DateOnly EndDate,
    string Reason);

public record ReservationRequestDto(
    Guid Id,
    Guid VehicleId,
    string VehicleName,
    Guid ListingId,
    string RequesterName,
    string RequesterEmail,
    string? RequesterPhone,
    DateOnly StartDate,
    DateOnly EndDate,
    string? Comment,
    string Status,
    string? ResponseNote,
    DateTimeOffset CreatedAt,
    DateTimeOffset? RespondedAt);

public record MyReservationDto(
    Guid Id,
    Guid VehicleId,
    string VehicleName,
    Guid ListingId,
    string ListingTitleEn,
    string ListingTitleEs,
    DateOnly StartDate,
    DateOnly EndDate,
    string? Comment,
    string Status,
    string? ResponseNote,
    DateTimeOffset CreatedAt,
    DateTimeOffset? RespondedAt);

public record CreateReservationRequest(
    DateOnly StartDate,
    DateOnly EndDate,
    string RequesterName,
    string RequesterEmail,
    string? RequesterPhone,
    string? Comment);

public record ReservationDecisionRequest(string? Note);

public record MyListingDto(
    Guid Id,
    string Slug,
    string TitleEn,
    string TitleEs,
    string CategorySlug,
    string Status,
    string Tier,
    bool HasPublishedSnapshot,
    bool IsPausedByOwner,
    DateTimeOffset? ApprovedAt,
    DateTimeOffset UpdatedAt);

/// <summary>
/// A GM-approved review.
/// </summary>
public record ReviewDto(
    string        ReviewerName,
    decimal       Rating,
    string        Text,
    DateTimeOffset CreatedAt
);

/// <summary>
/// Breakdown of ratings by star value.
/// </summary>
public record RatingDistributionDto(
    int FiveStar,
    int FourStar,
    int ThreeStar,
    int TwoStar,
    int OneStar
);

/// <summary>
/// Filter state for the Browse/Listings page.
/// Null means "no filter applied" for that dimension.
/// </summary>
public record ListingFilters(
    decimal? MinRating,         // null | 4.0m | 4.5m
    bool?    EnglishOnly,       // null = any, true = english, false = exclude english
    bool?    SpanishOnly,       // null = any, true = spanish, false = exclude spanish
    bool?    IncludeVip,        // tier checkboxes (null = not set = include all)
    bool?    IncludeVerified,
    bool     VerifiedOnly       // toggle switch
)
{
    public static ListingFilters Default => new(
        MinRating:       null,
        EnglishOnly:     null,
        SpanishOnly:     null,
        IncludeVip:      null,
        IncludeVerified: null,
        VerifiedOnly:    true
    );

    /// <summary>
    /// Derives the tier query param value from tier checkboxes.
    /// null and false are treated identically (checkbox unchecked).
    /// Only returns a value when exactly one tier is selected.
    /// </summary>
    public string? TierParam
    {
        get
        {
            bool hasVip      = IncludeVip      == true;
            bool hasVerified = IncludeVerified == true;

            // Only one selected: filter by that tier
            if (hasVip && !hasVerified) return "VIP";
            if (!hasVip && hasVerified) return "Verified";
            // Neither or both: no tier filter
            return null;
        }
    }

    /// <summary>
    /// Derives the language query param from language checkboxes.
    /// null and false are treated identically (checkbox unchecked).
    /// Only returns a value when exactly one language is selected.
    /// </summary>
    public string? LanguageParam
    {
        get
        {
            bool hasEn = EnglishOnly == true;
            bool hasEs = SpanishOnly == true;

            if (hasEn && !hasEs) return "en";
            if (!hasEn && hasEs) return "es";
            return null;
        }
    }
}

/// <summary>
/// Returned when a user clicks "Contact Provider" —
/// contains the WhatsApp deep-link to open.
/// </summary>
public record ContactResultDto(
    string  WhatsAppUrl,
    bool    WaiverRequired,
    string? WaiverText
);

/// <summary>
/// Successful login response.
/// </summary>
public record LoginResultDto(
    string AccessToken,
    string TokenType,
    int    ExpiresIn,
    string Role,
    string Email
);

/// <summary>
/// Successful registration response.
/// </summary>
public record UserRegisteredDto(
    Guid   UserId,
    string Email,
    string Role
);

// ============================================================
// Manage-panel DTOs (owner-facing, /manage routes)
// ============================================================

/// <summary>
/// Extended owner listing DTO — mirrors the backend MyListingDto after B2 extension.
/// Includes editable fields so ManageDetailOverview can pre-populate forms.
/// </summary>
public record MyListingExtendedDto(
    Guid             Id,
    string           Slug,
    string           TitleEn,
    string           TitleEs,
    string           DescriptionEn,
    string           DescriptionEs,
    string           CategorySlug,
    string           Status,
    string           Tier,
    bool             HasPublishedSnapshot,
    string?          WhatsAppNumber,
    string?          Location,
    decimal?         Price,
    DateTimeOffset?  ApprovedAt,
    DateTimeOffset   UpdatedAt
);

/// <summary>
/// Request body for PUT /api/v1/listings/{id}.
/// Updating any field forces the listing back to Pending (Regla #5).
/// </summary>
public record UpdateListingRequest(
    string  TitleEn,
    string  TitleEs,
    string  DescriptionEn,
    string  DescriptionEs,
    string? WhatsAppNumber,
    string? Location,
    decimal? Price
);

/// <summary>
/// KPI summary for the owner's listing dashboard.
/// Mirrors Application.DTOs.ListingAnalyticsDto.
/// Delta fields are null when no previous period data is available.
/// </summary>
public record ListingAnalyticsDto(
    int                    ViewsTotal,
    int                    ViewsToday,
    int                    ViewsThisWeek,
    int                    ViewsThisMonth,
    IReadOnlyList<DailyViewDto> DailyViews,
    int                    ContactsTotal,
    int                    WhatsAppClicksTotal,
    decimal                AvgRating,
    int                    TotalRatings,
    int?                   ViewsMonthDeltaPercent,
    int?                   WhatsAppContactsDeltaPercent
);

public record DailyViewDto(DateOnly Date, int Count);

/// <summary>
/// Owner-panel reservation row. Mirrors Application.DTOs.ReservationRequestDto.
/// Renamed to avoid clash with the existing ReservationRequestDto (requester-side shape is identical).
/// </summary>
public record ReservationDto(
    Guid              Id,
    Guid              VehicleId,
    string            VehicleName,
    Guid              ListingId,
    string            RequesterName,
    string            RequesterEmail,
    string?           RequesterPhone,
    DateOnly          StartDate,
    DateOnly          EndDate,
    string?           Comment,
    string            Status,
    string?           ResponseNote,
    DateTimeOffset    CreatedAt,
    DateTimeOffset?   RespondedAt
);

/// <summary>
/// A single approved review visible in the owner's listing dashboard.
/// Mirrors Application.DTOs.ListingReviewDto.
/// </summary>
public record ListingReviewDto(
    Guid           Id,
    string         ReviewerName,
    decimal        Rating,
    string?        Comment,
    DateTimeOffset CreatedAt,
    bool           IsVerified
);
