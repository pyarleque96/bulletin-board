using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Exceptions;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Bulletin.Board.Infrastructure.Persistence.Repositories;

public sealed class VehicleRepository(ApplicationDbContext context)
    : Repository<Vehicle>(context), IVehicleRepository
{
    public async Task<IReadOnlyList<Vehicle>> GetByListingAsync(Guid listingId, CancellationToken ct = default)
        => await DbSet
            .Where(v => v.ListingId == listingId && v.IsActive)
            .Include(v => v.Photos.Where(p => p.IsPublic))
            .OrderBy(v => v.DisplayOrder)
            .ThenBy(v => v.CreatedAt)
            .ToListAsync(ct);

    public async Task<Vehicle?> GetWithDetailsAsync(Guid vehicleId, CancellationToken ct = default)
        => await DbSet
            .Where(v => v.Id == vehicleId)
            .Include(v => v.Photos)
            .Include(v => v.Unavailability)
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<VehicleUnavailability>> GetUnavailabilityInRangeAsync(
        Guid vehicleId, DateOnly from, DateOnly to, CancellationToken ct = default)
        => await context.VehicleUnavailability
            .Where(u => u.VehicleId == vehicleId
                        && u.StartDate <= to
                        && u.EndDate >= from)
            .OrderBy(u => u.StartDate)
            .ToListAsync(ct);

    public async Task<VehiclePhoto?> GetPhotoByIdAsync(Guid photoId, CancellationToken ct = default)
        => await context.VehiclePhotos
            .Include(p => p.Vehicle)
            .FirstOrDefaultAsync(p => p.Id == photoId, ct);

    public async Task AddPhotoAsync(VehiclePhoto photo, CancellationToken ct = default)
        => await context.VehiclePhotos.AddAsync(photo, ct);

    public void RemovePhoto(VehiclePhoto photo)
        => context.VehiclePhotos.Remove(photo);

    public async Task UnsetPrimaryForAllPhotosAsync(Guid vehicleId, CancellationToken ct = default)
    {
        // Bulk update — avoids loading rows we don't need.
        await context.VehiclePhotos
            .Where(p => p.VehicleId == vehicleId && p.IsPrimary)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.IsPrimary, false), ct);
    }

    public async Task<IReadOnlyList<Vehicle>> GetByListingForOwnerAsync(Guid listingId, CancellationToken ct = default)
        => await DbSet
            .Where(v => v.ListingId == listingId)
            .Include(v => v.Photos)
            .OrderBy(v => v.DisplayOrder)
            .ThenBy(v => v.Id)
            .ToListAsync(ct);

    public async Task<(IReadOnlyList<Vehicle> Items, int Total)> GetByListingForOwnerPagedAsync(
        Guid listingId, int skip, int take, CancellationToken ct = default)
    {
        var baseQuery = DbSet
            .Where(v => v.ListingId == listingId)
            .AsNoTracking();

        var total = await baseQuery.CountAsync(ct);

        var items = await baseQuery
            .Include(v => v.Photos)
            .OrderBy(v => v.DisplayOrder)
            .ThenBy(v => v.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<VehicleUnavailability?> GetUnavailabilityByIdAsync(
        Guid unavailabilityId, CancellationToken ct = default)
        => await context.VehicleUnavailability
            .Include(u => u.Vehicle)
            .FirstOrDefaultAsync(u => u.Id == unavailabilityId, ct);

    public async Task AddUnavailabilityAsync(VehicleUnavailability unavailability, CancellationToken ct = default)
    {
        await context.VehicleUnavailability.AddAsync(unavailability, ct);
        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsOverlapViolation(ex))
        {
            // Detach the failed entity so a retry with a fresh instance doesn't get the
            // "already tracked" error.
            context.VehicleUnavailability.Entry(unavailability).State = EntityState.Detached;
            throw new VehicleUnavailabilityOverlapException(
                "Selected dates overlap an existing blackout. Pick a different range.");
        }
    }

    private static bool IsOverlapViolation(DbUpdateException ex)
        => ex.InnerException is PostgresException pg
           && pg.SqlState == "23P01"  // exclusion_violation
           && pg.ConstraintName == "excl_vehicle_unavailability_no_overlap";

    public void RemoveUnavailability(VehicleUnavailability unavailability)
        => context.VehicleUnavailability.Remove(unavailability);
}
