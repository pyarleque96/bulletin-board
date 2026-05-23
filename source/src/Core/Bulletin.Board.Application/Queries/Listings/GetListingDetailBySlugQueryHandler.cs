using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Application.Snapshots;
using Bulletin.Board.Domain.Common;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Queries.Listings;

public sealed class GetListingDetailBySlugQueryHandler(
    IListingRepository listingRepository,
    IListingSnapshotBuilder snapshotBuilder,
    ILogger<GetListingDetailBySlugQueryHandler> logger)
    : IRequestHandler<GetListingDetailBySlugQuery, ListingDetailDto?>
{
    public async Task<ListingDetailDto?> Handle(GetListingDetailBySlugQuery request, CancellationToken ct)
    {
        var normalized = SlugGenerator.Slugify(request.Slug);

        logger.LogInformation("Fetching listing detail for slug {Slug}", normalized);

        var listing = await listingRepository.GetDetailBySlugAsync(normalized, ct);
        if (listing is null || listing.IsDeleted)
            return null;

        var snapshot = snapshotBuilder.Parse(listing.PublishedSnapshot);

        // Same visibility rule as id-based handler — see comment there.
        if (snapshot is null && listing.Status != ListingStatus.Approved)
            return null;

        return await ListingDetailMapper.BuildAsync(listing, snapshot, listingRepository, ct);
    }
}
