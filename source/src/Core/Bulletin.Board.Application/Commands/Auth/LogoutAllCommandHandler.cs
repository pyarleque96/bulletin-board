using Bulletin.Board.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Commands.Auth;

public sealed class LogoutAllCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    ILogger<LogoutAllCommandHandler> logger)
    : IRequestHandler<LogoutAllCommand, Unit>
{
    public async Task<Unit> Handle(LogoutAllCommand request, CancellationToken ct)
    {
        await refreshTokenRepository.RevokeAllForUserAsync(request.UserId, ct);
        logger.LogInformation("All refresh tokens revoked for user {UserId}", request.UserId);
        return Unit.Value;
    }
}
