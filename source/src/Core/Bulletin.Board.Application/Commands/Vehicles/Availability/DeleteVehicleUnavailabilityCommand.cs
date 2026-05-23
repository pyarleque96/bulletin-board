using MediatR;

namespace Bulletin.Board.Application.Commands.Vehicles.Availability;

public record DeleteVehicleUnavailabilityCommand(Guid UnavailabilityId) : IRequest;
