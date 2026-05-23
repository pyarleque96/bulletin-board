using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Queries.Listings;

public sealed class SearchListingsQueryHandler(
    IListingRepository listingRepository,
    IRepository<Category> categoryRepository,
    ILogger<SearchListingsQueryHandler> logger)
    : IRequestHandler<SearchListingsQuery, PagedResult<ListingCardDto>>
{
    public async Task<PagedResult<ListingCardDto>> Handle(SearchListingsQuery request, CancellationToken ct)
    {
        logger.LogInformation("Searching listings with category={Category} tier={Tier}", request.Category, request.Tier);

        Guid? categoryId = null;
        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            var categories = await categoryRepository.FindAsync(c => c.Slug == request.Category && c.IsActive, ct);
            categoryId = categories.FirstOrDefault()?.Id;
        }

        var allApproved = await listingRepository.GetRankedAsync(
            categoryId: categoryId,
            status: ListingStatus.Approved,
            page: 1,
            pageSize: 1000,
            ct);

        var filtered = allApproved.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(request.Tier) && Enum.TryParse<ProviderTier>(request.Tier, out var tier))
            filtered = filtered.Where(l => l.ProviderTier == tier);

        if (!string.IsNullOrWhiteSpace(request.Language))
        {
            filtered = request.Language.ToLower() switch
            {
                "en" => filtered.Where(l => !string.IsNullOrWhiteSpace(l.TitleEn)),
                "es" => filtered.Where(l => !string.IsNullOrWhiteSpace(l.TitleEs)),
                _ => filtered
            };
        }

        if (request.MinRating.HasValue)
            filtered = filtered.Where(l => (l.AvgRating ?? 0) >= request.MinRating.Value);

        // El orden correcto viene del repositorio (Hierarchy > Tier > AvgRating > CreatedAt > Id).
        // Solo se aplican filtros adicionales aqui; no se re-ordena en memoria.
        var sorted = filtered.ToList();

        var totalCount = sorted.Count;

        var paged = sorted
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(ListingCardMapper.ToCardDto)
            .ToList();

        return new PagedResult<ListingCardDto>(paged, totalCount, request.Page, request.PageSize);
    }
}
