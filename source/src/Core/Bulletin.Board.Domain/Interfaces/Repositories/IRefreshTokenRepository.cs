using Bulletin.Board.Domain.Entities;

namespace Bulletin.Board.Domain.Interfaces.Repositories;

/// <summary>
/// Repositorio especializado para RefreshToken que expone operaciones
/// necesarias para la rotación por familia y detección de reuse.
/// </summary>
public interface IRefreshTokenRepository
{
    /// <summary>Busca un token por su hash SHA-256.</summary>
    Task<RefreshToken?> FindByHashAsync(string tokenHash, CancellationToken ct = default);

    /// <summary>
    /// Devuelve todos los tokens de una familia (activos y revocados).
    /// Usado en la detección de reuse para revocar toda la familia.
    /// </summary>
    Task<IReadOnlyList<RefreshToken>> FindByFamilyAsync(Guid familyId, CancellationToken ct = default);

    /// <summary>Revoca todos los tokens de una familia. Operación bulk.</summary>
    Task RevokeFamilyAsync(Guid familyId, CancellationToken ct = default);

    /// <summary>Revoca todos los tokens activos de un usuario (logout-all).</summary>
    Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default);

    Task AddAsync(RefreshToken token, CancellationToken ct = default);

    void Update(RefreshToken token);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
