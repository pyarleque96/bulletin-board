using MediatR;

namespace Bulletin.Board.Application.Commands.Auth;

/// <summary>
/// Revoca todos los refresh tokens activos del usuario identificado por <see cref="UserId"/>.
/// Equivale a cerrar todas las sesiones abiertas en todos los dispositivos.
/// </summary>
public record LogoutAllCommand(Guid UserId) : IRequest<Unit>;
