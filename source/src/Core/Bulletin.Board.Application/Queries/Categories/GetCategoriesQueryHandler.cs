using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Queries.Categories;

public sealed class GetCategoriesQueryHandler(
    ICategoryRepository categoryRepository,
    ILogger<GetCategoriesQueryHandler> logger)
    : IRequestHandler<GetCategoriesQuery, CategoryDto[]>
{
    public async Task<CategoryDto[]> Handle(GetCategoriesQuery request, CancellationToken ct)
    {
        logger.LogInformation("Fetching active categories (includeCount={IncludeCount})", request.IncludeCount);

        if (request.IncludeCount)
        {
            var withCounts = await categoryRepository.GetWithListingCountsAsync(ct);

            return withCounts
                .Select(r => new CategoryDto(
                    Id: r.Category.Id,
                    NameEn: r.Category.NameEn,
                    NameEs: r.Category.NameEs,
                    Slug: r.Category.Slug,
                    Icon: r.Category.IconUrl,
                    ListingCount: r.ApprovedListingCount))
                .ToArray();
        }

        var categories = await categoryRepository.FindAsync(c => c.IsActive, ct);

        return categories
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new CategoryDto(
                Id: c.Id,
                NameEn: c.NameEn,
                NameEs: c.NameEs,
                Slug: c.Slug,
                Icon: c.IconUrl,
                ListingCount: null))
            .ToArray();
    }
}
