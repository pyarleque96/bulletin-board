using Asp.Versioning;
using Bulletin.Board.Application.Commands.Admin.Images;
using Bulletin.Board.Application.Commands.Admin.Photos;
using Bulletin.Board.Application.DTOs.Admin;
using Bulletin.Board.Application.Queries.Admin.Images;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bulletin.Board.API.Controllers.V1.Admin;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/listings/{listingId:guid}/images")]
[Authorize(Roles = "Admin")]
public class AdminImagesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AdminImageDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AdminImageDto>>> GetAll(Guid listingId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetListingImagesQuery(listingId), ct);
        return Ok(result);
    }

    [HttpPatch("{imageId:guid}/visibility")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> SetVisibility(
        Guid listingId,
        Guid imageId,
        [FromBody] SetImageVisibilityRequest request,
        CancellationToken ct)
    {
        await mediator.Send(new SetImageVisibilityCommand(listingId, imageId, request.IsPublic), ct);
        return NoContent();
    }

    /// <summary>
    /// Manage v3 moderation endpoint (regla #4). Body: <c>{ Status: "Public"|"Hidden"|"Rejected", GmFeedback?: string }</c>.
    /// Preferido sobre <c>SetVisibility</c> que solo soporta booleano.
    /// </summary>
    [HttpPatch("{imageId:guid}/moderate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Moderate(
        Guid listingId,
        Guid imageId,
        [FromBody] ModeratePhotoRequest request,
        CancellationToken ct)
    {
        await mediator.Send(new ModeratePhotoCommand(imageId, request.Status, request.GmFeedback), ct);
        return NoContent();
    }

    [HttpDelete("{imageId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid listingId, Guid imageId, CancellationToken ct)
    {
        await mediator.Send(new DeleteImageCommand(listingId, imageId), ct);
        return NoContent();
    }
}

public record SetImageVisibilityRequest(bool IsPublic);
public record ModeratePhotoRequest(string Status, string? GmFeedback);
