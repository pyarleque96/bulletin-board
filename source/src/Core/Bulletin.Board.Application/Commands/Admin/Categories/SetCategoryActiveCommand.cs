using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Commands.Admin.Categories;

public record SetCategoryActiveCommand(Guid CategoryId, bool IsActive) : IRequest;

public sealed class SetCategoryActiveCommandHandler(
    IRepository<Category> categoryRepository,
    ICurrentUserService currentUserService,
    ILogger<SetCategoryActiveCommandHandler> logger)
    : IRequestHandler<SetCategoryActiveCommand>
{
    public async Task Handle(SetCategoryActiveCommand request, CancellationToken ct)
    {
        var category = await categoryRepository.GetByIdAsync(request.CategoryId, ct)
            ?? throw new InvalidOperationException($"Category '{request.CategoryId}' not found.");

        var adminId = currentUserService.UserId ?? throw new UnauthorizedAccessException();

        category.SetActive(request.IsActive);
        categoryRepository.Update(category);
        await categoryRepository.SaveChangesAsync(ct);

        logger.LogInformation("Category {CategoryId} active={IsActive} set by {AdminId}",
            request.CategoryId, request.IsActive, adminId);
    }
}

public sealed class SetCategoryActiveCommandValidator : AbstractValidator<SetCategoryActiveCommand>
{
    public SetCategoryActiveCommandValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty();
    }
}
