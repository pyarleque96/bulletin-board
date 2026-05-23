namespace Bulletin.Board.Application.DTOs;

public record RegisterUserDto(string Email, string Password, string Role);
public record LoginDto(string Email, string Password);

/// <summary>
/// Resultado de login/refresh. RefreshTokenRaw se usa SOLO internamente entre handler y controlador
/// para colocarlo en la cookie HttpOnly. Nunca se serializa en la respuesta JSON al cliente.
/// </summary>
public record LoginResultDto(string AccessToken, int ExpiresInSeconds, string RefreshTokenRaw = "");

public record UserRegisteredDto(Guid UserId);
