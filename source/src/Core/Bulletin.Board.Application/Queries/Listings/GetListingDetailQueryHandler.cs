using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Application.Snapshots;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Queries.Listings;

public sealed class GetListingDetailQueryHandler(
    IListingRepository listingRepository,
    IListingSnapshotBuilder snapshotBuilder,
    ILogger<GetListingDetailQueryHandler> logger)
    : IRequestHandler<GetListingDetailQuery, ListingDetailDto?>
{
    public async Task<ListingDetailDto?> Handle(GetListingDetailQuery request, CancellationToken ct)
    {
        logger.LogInformation("Fetching listing detail for {ListingId}", request.ListingId);

        var listing = await listingRepository.GetDetailAsync(request.ListingId, ct);
        if (listing is null || listing.IsDeleted)
            return null;

        var snapshot = snapshotBuilder.Parse(listing.PublishedSnapshot);

        // Visibility rule:
        //   - Snapshot present: public regardless of current Status (provider may be editing).
        //   - No snapshot + Status == Approved: legacy/seed listings — fall back to live entity.
        //   - Otherwise: never been approved → 404.
        if (snapshot is null && listing.Status != ListingStatus.Approved)
            return null;

        return await ListingDetailMapper.BuildAsync(listing, snapshot, listingRepository, ct);
    }
}
