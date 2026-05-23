using MediatR;

namespace Bulletin.Board.Application.Commands.Vehicles;

public record SetVehicleActiveCommand(Guid VehicleId, bool IsActive) : IRequest;
