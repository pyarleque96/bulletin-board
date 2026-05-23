using Bulletin.Board.Domain.Entities;

namespace Bulletin.Board.Domain.Interfaces.Repositories;

public interface IVehicleRepository : IRepository<Vehicle>
{
    /// <summary>
    /// Returns active vehicles for a listing in display order, with public photos eagerly
    /// loaded. Used by the VIP detail layout.
    /// </summary>
    Task<IReadOnlyList<Vehicle>> GetByListingAsync(Guid listingId, CancellationToken ct = default);

    /// <summary>
    /// Returns the vehicle with all photos and blackouts loaded. Used by the owner panel.
    /// </summary>
    Task<Vehicle?> GetWithDetailsAsync(Guid vehicleId, CancellationToken ct = default);

    /// <summary>
    /// Returns blackouts for a vehicle that intersect [from, to]. Used to render the calendar.
    /// </summary>
    Task<IReadOnlyList<VehicleUnavailability>> GetUnavailabilityInRangeAsync(
        Guid vehicleId, DateOnly from, DateOnly to, CancellationToken ct = default);

    /// <summary>Loads a vehicle photo for mutation flows.</summary>
    Task<VehiclePhoto?> GetPhotoByIdAsync(Guid photoId, CancellationToken ct = default);

    /// <summary>Adds a photo entity; caller manages SaveChanges.</summary>
    Task AddPhotoAsync(VehiclePhoto photo, CancellationToken ct = default);

    /// <summary>Removes a photo entity; caller manages SaveChanges.</summary>
    void RemovePhoto(VehiclePhoto photo);

    /// <summary>
    /// Resets <c>is_primary = false</c> on every photo of a vehicle. Used before marking
    /// a different photo as primary so we never end up with two primaries on the same vehicle.
    /// </summary>
    Task UnsetPrimaryForAllPhotosAsync(Guid vehicleId, CancellationToken ct = default);

    /// <summary>Returns vehicles for a listing including inactive ones and ALL photos (public+private). Owner view.</summary>
    Task<IReadOnlyList<Vehicle>> GetByListingForOwnerAsync(Guid listingId, CancellationToken ct = default);

    /// <summary>
    /// Paginates vehicles for a listing (owner view). Orden estable: DisplayOrder ASC, Id ASC.
    /// Returns (page items, total count).
    /// </summary>
    Task<(IReadOnlyList<Vehicle> Items, int Total)> GetByListingForOwnerPagedAsync(
        Guid listingId, int skip, int take, CancellationToken ct = default);

    /// <summary>Loads a single unavailability (blackout) row for mutation flows.</summary>
    Task<VehicleUnavailability?> GetUnavailabilityByIdAsync(Guid unavailabilityId, CancellationToken ct = default);

    /// <summary>
    /// Adds a blackout and persists immediately. Translates the PostgreSQL EXCLUDE
    /// constraint violation into <see cref="Bulletin.Board.Domain.Exceptions.VehicleUnavailabilityOverlapException"/>
    /// so the caller doesn't need to know about Npgsql.
    /// </summary>
    Task AddUnavailabilityAsync(VehicleUnavailability unavailability, CancellationToken ct = default);

    /// <summary>Removes a blackout; caller manages SaveChanges.</summary>
    void RemoveUnavailability(VehicleUnavailability unavailability);
}
