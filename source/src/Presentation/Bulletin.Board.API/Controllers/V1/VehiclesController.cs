using Asp.Versioning;
using Bulletin.Board.Application.Commands.Vehicles;
using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Application.Queries.Vehicles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bulletin.Board.API.Controllers.V1;

/// <summary>
/// Vehicle management endpoints for car-rental listings. Authorization is enforced
/// inside each handler (listing owner OR Admin). Mutations revert the parent listing
/// to <c>Pending</c> per regla #5 del producto.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Authorize]
public class VehiclesController(IMediator mediator) : ControllerBase
{
    /// <summary>Lists the vehicles owned by the caller for a given listing, paginated. Owner view — includes
    /// inactive vehicles and ALL photos (public + private).</summary>
    [HttpGet("api/v{version:apiVersion}/listings/{listingId:guid}/vehicles")]
    [ProducesResponseType(typeof(PagedResult<OwnerVehicleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<OwnerVehicleDto>>> ListForOwner(
        Guid listingId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetOwnerVehiclesQuery(listingId, page, pageSize), ct);
        return Ok(result);
    }

    /// <summary>Creates a vehicle for a car-rental listing.</summary>
    [HttpPost("api/v{version:apiVersion}/listings/{listingId:guid}/vehicles")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Create(
        Guid listingId,
        [FromBody] CreateVehicleRequest request,
        CancellationToken ct)
    {
        var id = await mediator.Send(
            new CreateVehicleCommand(
                listingId,
                request.Name,
                request.PassengerMax,
                request.Transmission,
                request.HasAirConditioning,
                request.DailyRateCents,
                request.WeeklyRateCents,
                request.MonthlyRateCents,
                request.DisplayOrder),
            ct);

        return Created($"/api/v1/vehicles/{id}", new { id });
    }

    /// <summary>Updates a vehicle's specs and rates.</summary>
    [HttpPut("api/v{version:apiVersion}/vehicles/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Update(
        Guid id,
        [FromBody] UpdateVehicleRequest request,
        CancellationToken ct)
    {
        await mediator.Send(
            new UpdateVehicleCommand(
                id,
                request.Name,
                request.PassengerMax,
                request.Transmission,
                request.HasAirConditioning,
                request.DailyRateCents,
                request.WeeklyRateCents,
                request.MonthlyRateCents,
                request.DisplayOrder),
            ct);

        return NoContent();
    }

    /// <summary>Hard-deletes a vehicle. Cascades photos and unavailability rows.</summary>
    [HttpDelete("api/v{version:apiVersion}/vehicles/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteVehicleCommand(id), ct);
        return NoContent();
    }

    /// <summary>Activates or deactivates a vehicle without deleting it.</summary>
    [HttpPatch("api/v{version:apiVersion}/vehicles/{id:guid}/active")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> SetActive(
        Guid id,
        [FromBody] SetVehicleActiveRequest request,
        CancellationToken ct)
    {
        await mediator.Send(new SetVehicleActiveCommand(id, request.IsActive), ct);
        return NoContent();
    }
}

public record CreateVehicleRequest(
    string  Name,
    int     PassengerMax,
    string  Transmission,
    bool    HasAirConditioning,
    int     DailyRateCents,
    int?    WeeklyRateCents,
    int?    MonthlyRateCents,
    int     DisplayOrder);

public record UpdateVehicleRequest(
    string  Name,
    int     PassengerMax,
    string  Transmission,
    bool    HasAirConditioning,
    int     DailyRateCents,
    int?    WeeklyRateCents,
    int?    MonthlyRateCents,
    int     DisplayOrder);

public record SetVehicleActiveRequest(bool IsActive);
