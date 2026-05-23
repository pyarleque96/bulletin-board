using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Bulletin.Board.Domain.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Bulletin.Board.Infrastructure.Services;

/// <summary>
/// Implementación de IJwtTokenService usando HS256.
/// La signing key se lee de la variable de entorno JWT_SIGNING_KEY.
/// La validación de que la key tiene al menos 64 bytes se realiza en Program.cs al startup.
/// </summary>
public sealed class JwtTokenService(IConfiguration configuration) : IJwtTokenService
{
    private const string Issuer = "bulletin-dells";
    private const string Audience = "bulletin-dells-client";

    // 15 minutos según decisión de arquitectura
    public int AccessTokenExpirySeconds => 15 * 60;

    public (string Token, Guid Jti) GenerateAccessJwt(Guid userId, string email, IEnumerable<string> roles, string? name = null)
    {
        var key = GetSigningKey();
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var now = DateTimeOffset.UtcNow;
        var jti = Guid.NewGuid();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Jti, jti.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Iat, now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            // ClaimTypes.NameIdentifier para compatibilidad con ASP.NET Core auth middleware
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email)
        };

        if (!string.IsNullOrWhiteSpace(name))
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Name, name));
            claims.Add(new Claim(ClaimTypes.Name, name));
        }

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: now.AddSeconds(AccessTokenExpirySeconds).UtcDateTime,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), jti);
    }

    public (string Plain, string Hash) GenerateOpaqueRefreshToken()
    {
        // 32 bytes aleatorios → base64url (sin padding) para la cookie
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        var plain = Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

        var hash = HashToken(plain);
        return (plain, hash);
    }

    public string HashToken(string plain)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(plain));
        return Convert.ToBase64String(bytes);
    }

    private SymmetricSecurityKey GetSigningKey()
    {
        var keyValue = Environment.GetEnvironmentVariable("JWT_SIGNING_KEY")
            ?? configuration["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "JWT signing key is not configured. Set the JWT_SIGNING_KEY environment variable.");

        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyValue));
    }
}
