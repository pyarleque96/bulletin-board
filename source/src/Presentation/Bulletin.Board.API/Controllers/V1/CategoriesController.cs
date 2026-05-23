using Asp.Versioning;
using Bulletin.Board.Application.Commands.Categories;
using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Application.Queries.Categories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bulletin.Board.API.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/categories")]
public class CategoriesController(IMediator mediator) : ControllerBase
{
    /// <summary>Returns all active categories ordered by display order.</summary>
    /// <param name="includeCount">When true, each category includes the count of approved listings.</param>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetAll(
        [FromQuery] bool includeCount = false,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetCategoriesQuery(includeCount), ct);
        return Ok(result);
    }

    /// <summary>Creates a new category. Admin only.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Create([FromBody] CreateCategoryRequest request, CancellationToken ct)
    {
        var id = await mediator.Send(
            new CreateCategoryCommand(request.NameEn, request.NameEs, request.Slug, request.DisplayOrder, request.Icon),
            ct);

        return CreatedAtAction(nameof(GetAll), new { id }, new { id });
    }

    /// <summary>Updates an existing category. Admin only.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest request, CancellationToken ct)
    {
        await mediator.Send(
            new UpdateCategoryCommand(id, request.NameEn, request.NameEs, request.Slug, request.DisplayOrder, request.Icon, request.IsActive),
            ct);

        return NoContent();
    }

    /// <summary>Deletes a category. If listings exist, deactivates it instead. Admin only.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteCategoryCommand(id), ct);
        return NoContent();
    }
}

public record CreateCategoryRequest(string NameEn, string NameEs, string Slug, int DisplayOrder, string? Icon);
public record UpdateCategoryRequest(string NameEn, string NameEs, string Slug, int DisplayOrder, string? Icon, bool IsActive = true);
