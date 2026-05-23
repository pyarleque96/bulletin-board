using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Domain.Interfaces.Repositories;
using MediatR;

namespace Bulletin.Board.Application.Queries.Listings;

public record GetTopListingsQuery(int Count = 10) : IRequest<IReadOnlyList<ListingCardDto>>;

public sealed class GetTopListingsQueryHandler(IListingRepository listingRepository)
    : IRequestHandler<GetTopListingsQuery, IReadOnlyList<ListingCardDto>>
{
    private const int MaxPerProvider = 2;

    public async Task<IReadOnlyList<ListingCardDto>> Handle(GetTopListingsQuery request, CancellationToken ct)
    {
        var size = request.Count <= 0 ? 10 : Math.Min(request.Count, 50);

        // Over-fetch so the diversity cap (max-N per provider) has headroom to skip
        // listings without falling short of the requested count. Worst case: every fetched
        // listing belongs to the same provider — even with 3× the count we'd still under-deliver,
        // but that scenario is vanishingly rare in practice.
        var fetchSize = Math.Min(size * 3, 60);

        var listings = await listingRepository.GetTrendingAsync(fetchSize, ct);

        // Diversity cap: keep at most MaxPerProvider listings per ProviderId. Preserves the
        // upstream trending_score ordering — we only skip later occurrences once the cap hits.
        var perProvider = new Dictionary<Guid, int>();
        var result = new List<ListingCardDto>(size);
        foreach (var listing in listings)
        {
            if (result.Count >= size) break;
            var count = perProvider.GetValueOrDefault(listing.ProviderId);
            if (count >= MaxPerProvider) continue;
            perProvider[listing.ProviderId] = count + 1;
            result.Add(ListingCardMapper.ToCardDto(listing));
        }

        return result;
    }
}
