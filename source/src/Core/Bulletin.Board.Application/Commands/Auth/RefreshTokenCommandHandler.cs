using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Exceptions;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Commands.Auth;

/// <summary>
/// Implementa la rotación de refresh token con:
/// - Detección de reuse (token revocado presentado → revocar familia completa, SESSION_COMPROMISED)
/// - Sliding expiration: si restan menos de 7 días → extender a 30 días
/// - Inactividad implícita: si el token expiró sin haberse usado → TOKEN_EXPIRED
/// </summary>
public sealed class RefreshTokenCommandHandler(
    IUserService userService,
    IJwtTokenService jwtTokenService,
    IRefreshTokenRepository refreshTokenRepository,
    ILogger<RefreshTokenCommandHandler> logger)
    : IRequestHandler<RefreshTokenCommand, LoginResultDto>
{
    private static readonly TimeSpan SlidingThreshold = TimeSpan.FromDays(7);
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(30);

    public async Task<LoginResultDto> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        logger.LogInformation("Processing refresh token rotation");

        var tokenHash = jwtTokenService.HashToken(request.RefreshToken);
        var existing = await refreshTokenRepository.FindByHashAsync(tokenHash, ct);

        if (existing is null)
            throw new UnauthorizedAccessException("Refresh token not found.");

        // REUSE DETECTADO: el token fue encontrado pero ya está revocado.
        if (existing.RevokedAt is not null)
        {
            logger.LogWarning(
                "Refresh token reuse detected for family {FamilyId}, user {UserId}. Revoking entire family.",
                existing.FamilyId, existing.UserId);

            await refreshTokenRepository.RevokeFamilyAsync(existing.FamilyId, ct);
            throw new SessionCompromisedException(existing.FamilyId);
        }

        // Token expirado: inactividad >= 30 días.
        if (DateTimeOffset.UtcNow >= existing.ExpiresAt)
        {
            existing.Revoke();
            refreshTokenRepository.Update(existing);
            await refreshTokenRepository.SaveChangesAsync(ct);
            throw new TokenExpiredException();
        }

        // Token válido — rotación.
        var (email, roles, fullName) = await userService.GetUserInfoAsync(existing.UserId, ct);
        var (newAccessToken, newJti) = jwtTokenService.GenerateAccessJwt(existing.UserId, email, roles, fullName);

        // Sliding expiration: si restan < 7 días → extender a 30 días desde ahora.
        var timeRemaining = existing.ExpiresAt - DateTimeOffset.UtcNow;
        var newExpiry = timeRemaining < SlidingThreshold
            ? DateTimeOffset.UtcNow.Add(RefreshTokenLifetime)
            : existing.ExpiresAt;

        var (newRawToken, newTokenHash) = jwtTokenService.GenerateOpaqueRefreshToken();

        var newRefreshToken = RefreshToken.CreateReplacement(
            previous: existing,
            newTokenHash: newTokenHash,
            newExpiresAt: newExpiry,
            replacesJti: newJti);

        existing.Revoke();
        refreshTokenRepository.Update(existing);
        await refreshTokenRepository.AddAsync(newRefreshToken, ct);
        await refreshTokenRepository.SaveChangesAsync(ct);

        return new LoginResultDto(newAccessToken, jwtTokenService.AccessTokenExpirySeconds, newRawToken);
    }
}
