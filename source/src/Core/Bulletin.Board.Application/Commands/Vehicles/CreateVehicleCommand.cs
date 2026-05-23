using MediatR;

namespace Bulletin.Board.Application.Commands.Vehicles;

public record CreateVehicleCommand(
    Guid    ListingId,
    string  Name,
    int     PassengerMax,
    string  Transmission,         // "Manual" | "Automatic" | "Both"
    bool    HasAirConditioning,
    int     DailyRateCents,
    int?    WeeklyRateCents,
    int?    MonthlyRateCents,
    int     DisplayOrder
) : IRequest<Guid>;
