using Asp.Versioning;
using Bulletin.Board.Application.Commands.Reservations;
using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Application.Queries.Reservations;
using Bulletin.Board.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bulletin.Board.API.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Authorize]
public class ReservationsController(IMediator mediator) : ControllerBase
{
    /// <summary>Submits a reservation request against a vehicle. Requires login.</summary>
    [HttpPost("api/v{version:apiVersion}/vehicles/{vehicleId:guid}/reservations")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Create(
        Guid vehicleId,
        [FromBody] CreateReservationRequest request,
        CancellationToken ct)
    {
        var id = await mediator.Send(
            new CreateReservationRequestCommand(
                vehicleId,
                request.StartDate,
                request.EndDate,
                request.RequesterName,
                request.RequesterEmail,
                request.RequesterPhone,
                request.Comment),
            ct);
        return Created($"/api/v1/reservations/{id}", new { id });
    }

    /// <summary>Owner panel: lists reservations for a listing, optionally filtered by status, paginated.</summary>
    [HttpGet("api/v{version:apiVersion}/listings/{listingId:guid}/reservations")]
    [ProducesResponseType(typeof(PagedResult<ReservationRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<ReservationRequestDto>>> ListForListing(
        Guid listingId,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(
            new GetListingReservationsQuery(listingId, status, page, pageSize), ct);
        return Ok(result);
    }

    /// <summary>Requester history: all reservations submitted by the current user.</summary>
    [HttpGet("api/v{version:apiVersion}/me/reservations")]
    [ProducesResponseType(typeof(IReadOnlyList<MyReservationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<MyReservationDto>>> ListMine(CancellationToken ct)
    {
        var result = await mediator.Send(new GetMyReservationsQuery(), ct);
        return Ok(result);
    }

    /// <summary>Owner accepts a reservation. Auto-creates a Reserved blackout.</summary>
    [HttpPost("api/v{version:apiVersion}/reservations/{id:guid}/accept")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Accept(
        Guid id, [FromBody] ReservationDecisionRequest request, CancellationToken ct)
    {
        try
        {
            await mediator.Send(new AcceptReservationRequestCommand(id, request.Note), ct);
            return NoContent();
        }
        catch (VehicleUnavailabilityOverlapException ex)
        {
            return Problem(
                title: "Dates already booked",
                detail: ex.Message,
                statusCode: StatusCodes.Status409Conflict);
        }
    }

    /// <summary>Owner rejects a reservation.</summary>
    [HttpPost("api/v{version:apiVersion}/reservations/{id:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Reject(
        Guid id, [FromBody] ReservationDecisionRequest request, CancellationToken ct)
    {
        await mediator.Send(new RejectReservationRequestCommand(id, request.Note), ct);
        return NoContent();
    }

    /// <summary>Either side cancels. If was Accepted, the linked blackout is released.</summary>
    [HttpPost("api/v{version:apiVersion}/reservations/{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Cancel(
        Guid id, [FromBody] ReservationDecisionRequest request, CancellationToken ct)
    {
        await mediator.Send(new CancelReservationRequestCommand(id, request.Note), ct);
        return NoContent();
    }

    /// <summary>
    /// Owner marks a reservation as completed. Only valid from Accepted state.
    /// A DomainException from any other state results in 400.
    /// </summary>
    [HttpPost("api/v{version:apiVersion}/reservations/{id:guid}/mark-completed")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> MarkCompleted(Guid id, CancellationToken ct)
    {
        try
        {
            await mediator.Send(new MarkReservationCompletedCommand(id), ct);
            return NoContent();
        }
        catch (DomainException ex)
        {
            return Problem(
                title: "Invalid state transition",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest);
        }
    }

    /// <summary>
    /// Logs a WhatsApp contact interaction linked to a reservation.
    /// Regla #8: the GM always receives an email notification.
    /// </summary>
    [HttpPost("api/v{version:apiVersion}/reservations/{id:guid}/contact-log")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> LogContact(
        Guid id,
        [FromBody] ContactLogRequest request,
        CancellationToken ct)
    {
        await mediator.Send(new LogReservationContactCommand(id, request.Channel), ct);
        return NoContent();
    }
}

public record CreateReservationRequest(
    DateOnly StartDate,
    DateOnly EndDate,
    string RequesterName,
    string RequesterEmail,
    string? RequesterPhone,
    string? Comment);

public record ReservationDecisionRequest(string? Note);

public record ContactLogRequest(string Channel = "WhatsApp");
