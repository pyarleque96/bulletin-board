using MediatR;

namespace Bulletin.Board.Application.Commands.Categories;

public record DeleteCategoryCommand(Guid Id) : IRequest;
