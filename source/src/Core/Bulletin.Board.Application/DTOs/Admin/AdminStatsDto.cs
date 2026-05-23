namespace Bulletin.Board.Application.DTOs.Admin;

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
