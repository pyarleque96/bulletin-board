using MediatR;

namespace Bulletin.Board.Application.Commands.Vehicles;

public record UpdateVehicleCommand(
    Guid    VehicleId,
    string  Name,
    int     PassengerMax,
    string  Transmission,
    bool    HasAirConditioning,
    int     DailyRateCents,
    int?    WeeklyRateCents,
    int?    MonthlyRateCents,
    int     DisplayOrder
) : IRequest;
