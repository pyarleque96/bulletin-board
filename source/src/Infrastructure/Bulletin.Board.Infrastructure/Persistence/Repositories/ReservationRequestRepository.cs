using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Bulletin.Board.Infrastructure.Persistence.Repositories;

public sealed class ReservationRequestRepository(ApplicationDbContext context)
    : Repository<ReservationRequest>(context), IReservationRequestRepository
{
    public async Task<ReservationRequest?> GetWithRefsAsync(Guid id, CancellationToken ct = default)
        => await DbSet
            .Where(r => r.Id == id)
            .Include(r => r.Vehicle)
            .Include(r => r.Listing).ThenInclude(l => l.Provider)
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<ReservationRequest>> GetForListingAsync(
        Guid listingId, ReservationStatus? status, CancellationToken ct = default)
    {
        var query = DbSet.Where(r => r.ListingId == listingId);
        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        return await query
            .Include(r => r.Vehicle)
            .OrderByDescending(r => r.CreatedAt)
            .ThenBy(r => r.Id)
            .ToListAsync(ct);
    }

    public async Task<(IReadOnlyList<ReservationRequest> Items, int Total, bool HasAnyUnfiltered)>
        GetForListingPagedAsync(
            Guid listingId, ReservationStatus? status, int skip, int take, CancellationToken ct = default)
    {
        var baseQuery = DbSet
            .Where(r => r.ListingId == listingId)
            .AsNoTracking();

        var filtered = status.HasValue
            ? baseQuery.Where(r => r.Status == status.Value)
            : baseQuery;

        var total = await filtered.CountAsync(ct);

        // Sin filtro, total ya nos dice si existe alguna reserva. Con filtro, total solo cuenta el
        // subconjunto filtrado, por lo que necesitamos un probe SELECT EXISTS extra para que el
        // cliente pueda mostrar los pills aunque la página vigente venga vacía.
        bool hasAnyUnfiltered = status.HasValue
            ? await baseQuery.AnyAsync(ct)
            : total > 0;

        var items = await filtered
            .Include(r => r.Vehicle)
            .OrderByDescending(r => r.CreatedAt)
            .ThenBy(r => r.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

        return (items, total, hasAnyUnfiltered);
    }

    public async Task<IReadOnlyList<ReservationRequest>> GetForUserAsync(Guid userId, CancellationToken ct = default)
        => await DbSet
            .Where(r => r.RequesterUserId == userId)
            .Include(r => r.Vehicle)
            .Include(r => r.Listing)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

    public async Task<VehicleUnavailability?> FindBlackoutForReservationAsync(
        Guid reservationId, CancellationToken ct = default)
        => await context.VehicleUnavailability
            .FirstOrDefaultAsync(u => u.ReservationId == reservationId, ct);
}
