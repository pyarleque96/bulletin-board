using Asp.Versioning;
using Bulletin.Board.Application.Commands.Vehicles.Photos;
using Bulletin.Board.Application.Storage;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bulletin.Board.API.Controllers.V1;

/// <summary>
/// Vehicle photo endpoints. Owner manages add/delete/primary; only Admin (GM) can flip
/// public visibility per regla #4 del producto.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Authorize]
public class VehiclePhotosController(IMediator mediator, IBlobStorage blobStorage) : ControllerBase
{
    private static readonly string[] AllowedMimeTypes = ["image/jpeg", "image/png", "image/webp", "image/gif"];
    private const long MaxFileSizeBytes = 8 * 1024 * 1024;  // 8 MB

    /// <summary>
    /// Uploads a photo file (multipart/form-data) and attaches it to the vehicle.
    /// Returns the new photo id. Starts non-public until GM approves.
    /// </summary>
    [HttpPost("api/v{version:apiVersion}/vehicles/{vehicleId:guid}/photos/upload")]
    [RequestSizeLimit(MaxFileSizeBytes)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Upload(
        Guid vehicleId,
        [FromForm] IFormFile file,
        [FromForm] int displayOrder = 0,
        [FromForm] bool isPrimary = false,
        CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
            return Problem(title: "File required", statusCode: StatusCodes.Status400BadRequest);

        if (!AllowedMimeTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
            return Problem(
                title: "Unsupported content type",
                detail: $"Allowed: {string.Join(", ", AllowedMimeTypes)}",
                statusCode: StatusCodes.Status400BadRequest);

        var extension = Path.GetExtension(file.FileName)?.TrimStart('.') ?? "jpg";

        await using var stream = file.OpenReadStream();
        var url = await blobStorage.UploadAsync(
            "vehicle-photos", stream, file.ContentType, extension, ct);

        var id = await mediator.Send(
            new AddVehiclePhotoCommand(vehicleId, url, displayOrder, isPrimary), ct);

        return Created($"/api/v1/vehicle-photos/{id}", new { id, url });
    }

    /// <summary>Attaches a new photo (URL) to a vehicle. Photo starts non-public until GM approves.</summary>
    [HttpPost("api/v{version:apiVersion}/vehicles/{vehicleId:guid}/photos")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Add(
        Guid vehicleId,
        [FromBody] AddVehiclePhotoRequest request,
        CancellationToken ct)
    {
        var id = await mediator.Send(
            new AddVehiclePhotoCommand(vehicleId, request.FilePath, request.DisplayOrder, request.IsPrimary),
            ct);

        return Created($"/api/v1/vehicle-photos/{id}", new { id });
    }

    /// <summary>Removes a photo.</summary>
    [HttpDelete("api/v{version:apiVersion}/vehicle-photos/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteVehiclePhotoCommand(id), ct);
        return NoContent();
    }

    /// <summary>Promotes a photo as the primary one for its vehicle. Demotes any other primary.</summary>
    [HttpPatch("api/v{version:apiVersion}/vehicle-photos/{id:guid}/primary")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> SetPrimary(Guid id, CancellationToken ct)
    {
        await mediator.Send(new SetVehiclePhotoPrimaryCommand(id), ct);
        return NoContent();
    }

    /// <summary>Admin (GM) flips a photo's public visibility. Provider cannot self-publish.</summary>
    [HttpPatch("api/v{version:apiVersion}/vehicle-photos/{id:guid}/visibility")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> SetVisibility(
        Guid id,
        [FromBody] SetVehiclePhotoVisibilityRequest request,
        CancellationToken ct)
    {
        await mediator.Send(new SetVehiclePhotoVisibilityCommand(id, request.IsPublic), ct);
        return NoContent();
    }
}

public record AddVehiclePhotoRequest(string FilePath, int DisplayOrder, bool IsPrimary);
public record SetVehiclePhotoVisibilityRequest(bool IsPublic);
