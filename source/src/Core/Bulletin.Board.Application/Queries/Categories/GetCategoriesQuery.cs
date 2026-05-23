using Bulletin.Board.Application.DTOs;
using MediatR;

namespace Bulletin.Board.Application.Queries.Categories;

public record GetCategoriesQuery(bool IncludeCount = false) : IRequest<CategoryDto[]>;
