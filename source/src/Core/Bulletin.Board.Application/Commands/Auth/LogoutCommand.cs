using MediatR;

namespace Bulletin.Board.Application.Commands.Auth;

public record LogoutCommand(string? RefreshToken) : IRequest<Unit>;
