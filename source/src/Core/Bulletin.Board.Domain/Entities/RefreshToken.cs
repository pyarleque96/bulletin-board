namespace Bulletin.Board.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>
    /// Identifica la cadena de rotación a la que pertenece este token.
    /// Todos los tokens emitidos para la misma sesión comparten el mismo FamilyId.
    /// </summary>
    public Guid FamilyId { get; private set; }

    public Guid UserId { get; private set; }

    /// <summary>SHA-256 del token opaco. Nunca se almacena el token en plano.</summary>
    public string TokenHash { get; private set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    /// <summary>Jti del access token que reemplazó el token anterior de esta familia.</summary>
    public Guid? ReplacesJti { get; private set; }

    /// <summary>Información opcional del dispositivo (User-Agent truncado).</summary>
    public string? DeviceInfo { get; private set; }

    public bool IsActive => RevokedAt is null && DateTimeOffset.UtcNow < ExpiresAt;

    private RefreshToken() { }

    /// <summary>
    /// Crea el primer token de una nueva familia de sesión.
    /// </summary>
    public static RefreshToken Create(
        Guid userId,
        string tokenHash,
        DateTimeOffset expiresAt,
        string? deviceInfo = null)
        => new()
        {
            UserId = userId,
            FamilyId = Guid.NewGuid(),
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
            DeviceInfo = deviceInfo
        };

    /// <summary>
    /// Crea el token de reemplazo dentro de la misma familia (rotación).
    /// </summary>
    public static RefreshToken CreateReplacement(
        RefreshToken previous,
        string newTokenHash,
        DateTimeOffset newExpiresAt,
        Guid replacesJti)
        => new()
        {
            UserId = previous.UserId,
            FamilyId = previous.FamilyId,
            TokenHash = newTokenHash,
            ExpiresAt = newExpiresAt,
            ReplacesJti = replacesJti,
            DeviceInfo = previous.DeviceInfo
        };

    public void Revoke() => RevokedAt = DateTimeOffset.UtcNow;

    /// <summary>
    /// Extiende la expiración del token. Solo válido si el token está activo
    /// y la nueva expiración es mayor que la actual.
    /// </summary>
    public void Extend(DateTimeOffset newExpiry)
    {
        if (RevokedAt is not null)
            throw new InvalidOperationException("Cannot extend a revoked refresh token.");

        if (newExpiry <= ExpiresAt)
            throw new ArgumentException("New expiry must be greater than current expiry.", nameof(newExpiry));

        ExpiresAt = newExpiry;
    }
}
