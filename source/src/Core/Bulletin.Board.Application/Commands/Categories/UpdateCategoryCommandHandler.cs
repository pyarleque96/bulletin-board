using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Commands.Categories;

public sealed class UpdateCategoryCommandHandler(
    IRepository<Category> categoryRepository,
    ILogger<UpdateCategoryCommandHandler> logger)
    : IRequestHandler<UpdateCategoryCommand>
{
    public async Task Handle(UpdateCategoryCommand request, CancellationToken ct)
    {
        var category = await categoryRepository.GetByIdAsync(request.Id, ct);
        if (category is null)
            throw new InvalidOperationException($"Category '{request.Id}' not found.");

        // Verificar unicidad del slug si cambio
        if (category.Slug != request.Slug)
        {
            var existing = await categoryRepository.FindAsync(
                c => c.Slug == request.Slug && c.Id != request.Id, ct);

            if (existing.Count > 0)
                throw new InvalidOperationException($"A category with slug '{request.Slug}' already exists.");
        }

        category.Update(request.NameEn, request.NameEs, request.Slug, request.DisplayOrder, request.Icon);
        category.SetActive(request.IsActive);
        categoryRepository.Update(category);
        await categoryRepository.SaveChangesAsync(ct);

        logger.LogInformation("Category {Id} updated (slug: '{Slug}')", category.Id, category.Slug);
    }
}
