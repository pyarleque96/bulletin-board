using MediatR;

namespace Bulletin.Board.Application.Commands.Vehicles.Photos;

public record DeleteVehiclePhotoCommand(Guid PhotoId) : IRequest;
