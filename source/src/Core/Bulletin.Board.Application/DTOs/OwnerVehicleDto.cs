namespace Bulletin.Board.Application.DTOs;

/// <summary>
/// Owner-facing representation of a vehicle. Includes inactive vehicles and ALL photos
/// (including private/pending GM approval) so the provider sees what the GM still has to review.
/// </summary>
public record OwnerVehicleDto(
    Guid Id,
    string Name,
    int PassengerMax,
    string Transmission,
    bool HasAirConditioning,
    int DailyRateCents,
    int? WeeklyRateCents,
    int? MonthlyRateCents,
    int DisplayOrder,
    bool IsActive,
    OwnerVehiclePhotoDto[] Photos);

public record OwnerVehiclePhotoDto(
    Guid Id,
    string FilePath,
    bool IsPrimary,
    bool IsPublic,
    int DisplayOrder);
