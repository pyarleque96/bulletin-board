using Asp.Versioning;
using Bulletin.Board.Application.Commands.Admin.Providers;
using Bulletin.Board.Application.Commands.Providers;
using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Application.DTOs.Admin;
using Bulletin.Board.Application.Queries.Admin.Providers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bulletin.Board.API.Controllers.V1.Admin;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/providers")]
[Authorize(Roles = "Admin")]
public class AdminProvidersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AdminProviderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AdminProviderDto>>> GetAll(
        [FromQuery] string? tier,
        [FromQuery] string? verificationStatus,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDir = null,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(
            new GetAdminProvidersQuery(tier, verificationStatus, page, pageSize, sortBy, sortDir), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AdminProviderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminProviderDetailDto>> GetDetail(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetAdminProviderDetailQuery(id), ct);
        if (result is null)
            return Problem(title: "Provider not found", statusCode: StatusCodes.Status404NotFound);

        return Ok(result);
    }

    [HttpPut("{id:guid}/tier")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ChangeTier(Guid id, [FromBody] ChangeTierRequest request, CancellationToken ct)
    {
        await mediator.Send(new ChangeProviderTierCommand(id, request.Tier), ct);
        return NoContent();
    }

    [HttpPut("{id:guid}/hierarchy")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> SetHierarchy(Guid id, [FromBody] SetHierarchyAdminRequest request, CancellationToken ct)
    {
        await mediator.Send(new SetProviderHierarchyCommand(id, request.Hierarchy), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/verify")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Verify(Guid id, CancellationToken ct)
    {
        await mediator.Send(new VerifyProviderCommand(id), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/verification/phone")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> VerifyPhone(Guid id, CancellationToken ct)
    {
        await mediator.Send(new SetVerificationFlagCommand(id, VerificationFlagKind.Phone), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/verification/identity")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> VerifyIdentity(Guid id, CancellationToken ct)
    {
        await mediator.Send(new SetVerificationFlagCommand(id, VerificationFlagKind.Identity), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/verification/social")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> VerifySocial(Guid id, CancellationToken ct)
    {
        await mediator.Send(new SetVerificationFlagCommand(id, VerificationFlagKind.Social), ct);
        return NoContent();
    }
}

public record ChangeTierRequest(string Tier);
public record SetHierarchyAdminRequest(int Hierarchy);
