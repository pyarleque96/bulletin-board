namespace Bulletin.Board.Application.DTOs;

/// <summary>
/// Listing-side reservation summary (owner panel).
/// </summary>
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

/// <summary>
/// Requester-side reservation summary — same shape but adds the listing title for display.
/// </summary>
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
