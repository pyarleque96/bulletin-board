using Bulletin.Board.Application.DTOs.Admin;
using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Queries.Admin.Categories;

public record GetAdminCategoriesQuery : IRequest<IReadOnlyList<AdminCategoryDto>>;

public sealed class GetAdminCategoriesQueryHandler(
    IRepository<Category> categoryRepository,
    ILogger<GetAdminCategoriesQueryHandler> logger)
    : IRequestHandler<GetAdminCategoriesQuery, IReadOnlyList<AdminCategoryDto>>
{
    public async Task<IReadOnlyList<AdminCategoryDto>> Handle(GetAdminCategoriesQuery request, CancellationToken ct)
    {
        logger.LogInformation("Admin fetching all categories including inactive");

        var all = await categoryRepository.GetAllAsync(ct);

        return all
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new AdminCategoryDto(
                Id: c.Id,
                NameEn: c.NameEn,
                NameEs: c.NameEs,
                Slug: c.Slug,
                DisplayOrder: c.DisplayOrder,
                IsActive: c.IsActive,
                Icon: c.IconUrl))
            .ToList();
    }
}
