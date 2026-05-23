using MediatR;

namespace Bulletin.Board.Application.Commands.Vehicles.Photos;

public record SetVehiclePhotoPrimaryCommand(Guid PhotoId) : IRequest;
