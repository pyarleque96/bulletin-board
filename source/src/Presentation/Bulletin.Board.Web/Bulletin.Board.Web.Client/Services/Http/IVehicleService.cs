using Bulletin.Board.Web.Client.Models;
using Bulletin.Board.Web.Client.Services.Common;

namespace Bulletin.Board.Web.Client.Services.Http;

/// <summary>
/// Owner-facing vehicle service for the /manage/vehicles panel.
/// All methods return <see cref="ApiResult{T}"/> — never null, never throw on HTTP errors.
/// Note: mutating vehicles forces the parent listing back to Pending (Regla #5).
/// </summary>
public interface IVehicleService
{
    /// <summary>Paginated list of vehicles belonging to an owned listing.</summary>
    Task<ApiResult<PagedResultDto<OwnerVehicleDto>>> GetByListingAsync(
        Guid              listingId,
        int               page     = 1,
        int               pageSize = 20,
        CancellationToken ct       = default);

    /// <summary>Adds a new vehicle to the listing. Returns the new vehicle's Id on success.</summary>
    Task<ApiResult<Guid>> CreateAsync(
        Guid                  listingId,
        CreateVehicleRequest  request,
        CancellationToken     ct = default);

    /// <summary>Updates an existing vehicle.</summary>
    Task<ApiResult<bool>> UpdateAsync(
        Guid                  vehicleId,
        UpdateVehicleRequest  request,
        CancellationToken     ct = default);

    /// <summary>Toggles a vehicle's availability without triggering a full listing edit.</summary>
    Task<ApiResult<bool>> SetActiveAsync(
        Guid              vehicleId,
        bool              isActive,
        CancellationToken ct = default);

    /// <summary>Permanently deletes a vehicle.</summary>
    Task<ApiResult<bool>> DeleteAsync(
        Guid              vehicleId,
        CancellationToken ct = default);
}
