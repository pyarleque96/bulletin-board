using MediatR;

namespace Bulletin.Board.Application.Commands.Vehicles.Photos;

public record AddVehiclePhotoCommand(
    Guid VehicleId,
    string FilePath,
    int DisplayOrder,
    bool IsPrimary
) : IRequest<Guid>;
