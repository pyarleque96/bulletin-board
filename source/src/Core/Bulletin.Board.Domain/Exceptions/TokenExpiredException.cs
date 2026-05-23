namespace Bulletin.Board.Domain.Exceptions;

/// <summary>
/// Se lanza cuando el refresh token presentado existe en BD pero ya expiró.
/// </summary>
public sealed class TokenExpiredException()
    : DomainException("The refresh token has expired.")
{
    public string ErrorCode => "TOKEN_EXPIRED";
}
