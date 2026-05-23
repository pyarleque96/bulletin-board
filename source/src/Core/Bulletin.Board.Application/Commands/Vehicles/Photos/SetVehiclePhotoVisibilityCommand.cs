using MediatR;

namespace Bulletin.Board.Application.Commands.Vehicles.Photos;

/// <summary>
/// Admin-only. Toggles the public visibility of a vehicle photo.
/// Per regla #4 del producto: el GM decide qué fotos son públicas.
/// </summary>
public record SetVehiclePhotoVisibilityCommand(Guid PhotoId, bool IsPublic) : IRequest;
