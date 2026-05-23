using Asp.Versioning;
using Bulletin.Board.Application.Commands.Providers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bulletin.Board.API.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/providers")]
public class ProvidersController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Sets the hierarchy value for a provider. Higher values appear first in all listings.
    /// The GM uses this to control ranking (e.g., assign 9999 to their own provider to always appear first).
    /// Admin only.
    /// </summary>
    [HttpPut("{id:guid}/hierarchy")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> SetHierarchy(
        Guid id,
        [FromBody] SetHierarchyRequest request,
        CancellationToken ct)
    {
        await mediator.Send(new SetProviderHierarchyCommand(id, request.Hierarchy), ct);
        return NoContent();
    }
}

public record SetHierarchyRequest(int Hierarchy);
