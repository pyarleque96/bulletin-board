using Asp.Versioning;
using Bulletin.Board.Application.Commands.Admin.Categories;
using Bulletin.Board.Application.Commands.Categories;
using Bulletin.Board.Application.DTOs.Admin;
using Bulletin.Board.Application.Queries.Admin.Categories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bulletin.Board.API.Controllers.V1.Admin;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/categories")]
[Authorize(Roles = "Admin")]
public class AdminCategoriesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AdminCategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AdminCategoryDto>>> GetAll(CancellationToken ct)
    {
        var result = await mediator.Send(new GetAdminCategoriesQuery(), ct);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Create([FromBody] AdminCreateCategoryRequest request, CancellationToken ct)
    {
        var id = await mediator.Send(
            new CreateCategoryCommand(request.NameEn, request.NameEs, request.Slug, request.DisplayOrder, request.Icon),
            ct);

        return CreatedAtAction(nameof(GetAll), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Update(Guid id, [FromBody] AdminUpdateCategoryRequest request, CancellationToken ct)
    {
        await mediator.Send(
            new UpdateCategoryCommand(id, request.NameEn, request.NameEs, request.Slug, request.DisplayOrder, request.Icon, request.IsActive),
            ct);

        return NoContent();
    }

    [HttpPatch("{id:guid}/active")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> SetActive(Guid id, [FromBody] SetCategoryActiveRequest request, CancellationToken ct)
    {
        await mediator.Send(new SetCategoryActiveCommand(id, request.IsActive), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteCategoryCommand(id), ct);
        return NoContent();
    }
}

public record AdminCreateCategoryRequest(string NameEn, string NameEs, string Slug, int DisplayOrder, string? Icon);
public record AdminUpdateCategoryRequest(string NameEn, string NameEs, string Slug, int DisplayOrder, string? Icon, bool IsActive = true);
public record SetCategoryActiveRequest(bool IsActive);
