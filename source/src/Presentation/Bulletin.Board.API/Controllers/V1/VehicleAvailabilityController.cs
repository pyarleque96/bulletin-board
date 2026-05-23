using Asp.Versioning;
using Bulletin.Board.Application.Commands.Vehicles.Availability;
using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Application.Queries.Vehicles;
using Bulletin.Board.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bulletin.Board.API.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
public class VehicleAvailabilityController(IMediator mediator) : ControllerBase
{
    /// <summary>Returns the blackout ranges for a vehicle within [from, to]. Public endpoint.</summary>
    [HttpGet("api/v{version:apiVersion}/vehicles/{vehicleId:guid}/availability")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(VehicleAvailabilityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VehicleAvailabilityDto>> Get(
        Guid vehicleId,
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        CancellationToken ct)
    {
        if (to < from)
            return Problem(
                title: "Invalid range",
                detail: "Query parameter 'to' must be on or after 'from'.",
                statusCode: StatusCodes.Status400BadRequest);

        var result = await mediator.Send(new GetVehicleAvailabilityQuery(vehicleId, from, to), ct);
        if (result is null)
            return Problem(title: "Vehicle not found", statusCode: StatusCodes.Status404NotFound);

        return Ok(result);
    }

    /// <summary>Adds a blackout range. Owner only. Reason: "Manual" or "Maintenance".</summary>
    [HttpPost("api/v{version:apiVersion}/vehicles/{vehicleId:guid}/unavailability")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Add(
        Guid vehicleId,
        [FromBody] AddUnavailabilityRequest request,
        CancellationToken ct)
    {
        try
        {
            var id = await mediator.Send(
                new AddVehicleUnavailabilityCommand(
                    vehicleId, request.StartDate, request.EndDate, request.Reason),
                ct);
            return Created($"/api/v1/vehicle-unavailability/{id}", new { id });
        }
        catch (VehicleUnavailabilityOverlapException ex)
        {
            // Translate the exclusion-constraint violation into a 409 with a friendly message.
            return Problem(
                title: "Overlap with existing blackout",
                detail: ex.Message,
                statusCode: StatusCodes.Status409Conflict);
        }
    }

    /// <summary>Deletes a blackout. Owner only. Reserved blackouts cannot be deleted directly.</summary>
    [HttpDelete("api/v{version:apiVersion}/vehicle-unavailability/{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteVehicleUnavailabilityCommand(id), ct);
        return NoContent();
    }
}

public record AddUnavailabilityRequest(DateOnly StartDate, DateOnly EndDate, string Reason);
