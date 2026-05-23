using MediatR;

namespace Bulletin.Board.Application.Commands.Vehicles;

public record DeleteVehicleCommand(Guid VehicleId) : IRequest;
