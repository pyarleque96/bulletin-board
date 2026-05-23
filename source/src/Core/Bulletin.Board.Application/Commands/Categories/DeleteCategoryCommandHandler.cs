using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Commands.Categories;

public sealed class DeleteCategoryCommandHandler(
    IRepository<Category> categoryRepository,
    IRepository<Listing> listingRepository,
    ILogger<DeleteCategoryCommandHandler> logger)
    : IRequestHandler<DeleteCategoryCommand>
{
    public async Task Handle(DeleteCategoryCommand request, CancellationToken ct)
    {
        var category = await categoryRepository.GetByIdAsync(request.Id, ct);
        if (category is null)
            throw new InvalidOperationException($"Category '{request.Id}' not found.");

        // Invariante: si hay listings asociados, desactivar en vez de borrar
        var listings = await listingRepository.FindAsync(
            l => l.CategoryId == request.Id && !l.IsDeleted, ct);

        if (listings.Count > 0)
        {
            category.SetActive(false);
            categoryRepository.Update(category);
            await categoryRepository.SaveChangesAsync(ct);

            logger.LogInformation(
                "Category {Id} deactivated (has {Count} associated listings)",
                request.Id, listings.Count);

            return;
        }

        categoryRepository.Remove(category);
        await categoryRepository.SaveChangesAsync(ct);

        logger.LogInformation("Category {Id} permanently deleted", request.Id);
    }
}
