using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Commands.Auth;

public sealed class LogoutCommandHandler(
    IJwtTokenService jwtTokenService,
    IRefreshTokenRepository refreshTokenRepository,
    ILogger<LogoutCommandHandler> logger)
    : IRequestHandler<LogoutCommand, Unit>
{
    public async Task<Unit> Handle(LogoutCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return Unit.Value;

        var tokenHash = jwtTokenService.HashToken(request.RefreshToken);
        var existing = await refreshTokenRepository.FindByHashAsync(tokenHash, ct);

        // Idempotente: si el token no existe o ya fue revocado, éxito silencioso.
        if (existing is null || !existing.IsActive)
            return Unit.Value;

        existing.Revoke();
        refreshTokenRepository.Update(existing);
        await refreshTokenRepository.SaveChangesAsync(ct);

        logger.LogInformation("Refresh token revoked for user {UserId}", existing.UserId);
        return Unit.Value;
    }
}
