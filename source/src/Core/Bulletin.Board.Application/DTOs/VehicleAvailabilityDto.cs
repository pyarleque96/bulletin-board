namespace Bulletin.Board.Application.DTOs;

/// <summary>
/// Calendar payload: the queried date window and the list of blackout ranges within it.
/// Days NOT covered by any range in <see cref="Blackouts"/> are considered available.
/// </summary>
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
