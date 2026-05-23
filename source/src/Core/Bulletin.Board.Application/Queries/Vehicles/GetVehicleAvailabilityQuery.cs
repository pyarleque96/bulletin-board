using Bulletin.Board.Application.DTOs;
using MediatR;

namespace Bulletin.Board.Application.Queries.Vehicles;

public record GetVehicleAvailabilityQuery(
    Guid VehicleId,
    DateOnly From,
    DateOnly To
) : IRequest<VehicleAvailabilityDto?>;
