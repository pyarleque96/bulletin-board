using MediatR;

namespace Bulletin.Board.Application.Commands.Categories;

public record UpdateCategoryCommand(
    Guid Id,
    string NameEn,
    string NameEs,
    string Slug,
    int DisplayOrder,
    string? Icon,
    bool IsActive) : IRequest;
