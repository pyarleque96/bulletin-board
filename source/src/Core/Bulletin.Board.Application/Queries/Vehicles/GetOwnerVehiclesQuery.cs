using Bulletin.Board.Application.DTOs;
using MediatR;

namespace Bulletin.Board.Application.Queries.Vehicles;

public record GetOwnerVehiclesQuery(Guid ListingId, int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<OwnerVehicleDto>>;
