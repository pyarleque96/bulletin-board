using MediatR;

namespace Bulletin.Board.Application.Commands.Categories;

public record CreateCategoryCommand(
    string NameEn,
    string NameEs,
    string Slug,
    int DisplayOrder,
    string? Icon) : IRequest<Guid>;
