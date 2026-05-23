using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Commands.Auth;

public sealed class LoginCommandHandler(
    IUserService userService,
    IJwtTokenService jwtTokenService,
    IRefreshTokenRepository refreshTokenRepository,
    ILogger<LoginCommandHandler> logger)
    : IRequestHandler<LoginCommand, LoginResultDto>
{
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(30);

    public async Task<LoginResultDto> Handle(LoginCommand request, CancellationToken ct)
    {
        logger.LogInformation("Login attempt received");

        var (success, userId, email, roles) = await userService.ValidateCredentialsAsync(request.Email, request.Password, ct);
        if (!success)
            throw new UnauthorizedAccessException("Invalid credentials.");

        var (_, _, fullName) = await userService.GetUserInfoAsync(userId, ct);
        var (accessToken, _) = jwtTokenService.GenerateAccessJwt(userId, email, roles, fullName);

        var (rawToken, tokenHash) = jwtTokenService.GenerateOpaqueRefreshToken();

        var refreshToken = RefreshToken.Create(
            userId: userId,
            tokenHash: tokenHash,
            expiresAt: DateTimeOffset.UtcNow.Add(RefreshTokenLifetime),
            deviceInfo: request.DeviceInfo);

        await refreshTokenRepository.AddAsync(refreshToken, ct);
        await refreshTokenRepository.SaveChangesAsync(ct);

        return new LoginResultDto(accessToken, jwtTokenService.AccessTokenExpirySeconds, rawToken);
    }
}
