using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Commands.Categories;

public sealed class CreateCategoryCommandHandler(
    IRepository<Category> categoryRepository,
    ILogger<CreateCategoryCommandHandler> logger)
    : IRequestHandler<CreateCategoryCommand, Guid>
{
    public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken ct)
    {
        var existing = await categoryRepository.FindAsync(
            c => c.Slug == request.Slug, ct);

        if (existing.Count > 0)
            throw new InvalidOperationException($"A category with slug '{request.Slug}' already exists.");

        var category = Category.Create(
            nameEn: request.NameEn,
            nameEs: request.NameEs,
            slug: request.Slug,
            displayOrder: request.DisplayOrder,
            iconUrl: request.Icon);

        await categoryRepository.AddAsync(category, ct);
        await categoryRepository.SaveChangesAsync(ct);

        logger.LogInformation("Category '{Slug}' created with Id {Id}", category.Slug, category.Id);

        return category.Id;
    }
}
