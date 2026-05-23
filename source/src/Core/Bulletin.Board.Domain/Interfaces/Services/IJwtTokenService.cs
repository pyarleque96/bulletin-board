namespace Bulletin.Board.Domain.Interfaces.Services;

/// <summary>
/// Servicio para generación y utilidades de tokens de autenticación.
/// La signing key se lee de la variable de entorno JWT_SIGNING_KEY (nunca de appsettings.json).
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Genera un access JWT (HS256, 15 min) con los claims:
    /// sub, jti, email, role, name, iat, exp, iss=bulletin-dells, aud=bulletin-dells-client.
    /// </summary>
    /// <param name="userId">Identificador del usuario (claim sub).</param>
    /// <param name="email">Email del usuario.</param>
    /// <param name="roles">Roles del usuario.</param>
    /// <param name="name">Nombre completo del usuario (FirstName + LastName).</param>
    /// <returns>Tupla (JWT firmado, jti del token generado).</returns>
    (string Token, Guid Jti) GenerateAccessJwt(Guid userId, string email, IEnumerable<string> roles, string? name = null);

    /// <summary>
    /// Genera un refresh token opaco (32 bytes aleatorios en base64url).
    /// </summary>
    /// <returns>Tupla (tokenPlano, hashSha256). Solo el hash se almacena en BD.</returns>
    (string Plain, string Hash) GenerateOpaqueRefreshToken();

    /// <summary>Calcula SHA-256 del token en plano y lo devuelve en base64.</summary>
    string HashToken(string plain);

    /// <summary>Duración del access token en segundos (900 = 15 min).</summary>
    int AccessTokenExpirySeconds { get; }
}
