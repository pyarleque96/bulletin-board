using MediatR;

namespace Bulletin.Board.Application.Commands.Vehicles.Availability;

public record AddVehicleUnavailabilityCommand(
    Guid VehicleId,
    DateOnly StartDate,
    DateOnly EndDate,
    string Reason   // "Manual" | "Maintenance" — Reserved is system-managed
) : IRequest<Guid>;
