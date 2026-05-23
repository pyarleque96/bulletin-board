using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Bulletin.Board.Infrastructure.Persistence.Repositories;

public sealed class RefreshTokenRepository(ApplicationDbContext context) : IRefreshTokenRepository
{
    public async Task<RefreshToken?> FindByHashAsync(string tokenHash, CancellationToken ct = default)
        => await context.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

    public async Task<IReadOnlyList<RefreshToken>> FindByFamilyAsync(Guid familyId, CancellationToken ct = default)
        => await context.RefreshTokens
            .Where(t => t.FamilyId == familyId)
            .ToListAsync(ct);

    public async Task RevokeFamilyAsync(Guid familyId, CancellationToken ct = default)
    {
        var revokedAt = DateTimeOffset.UtcNow;
        await context.RefreshTokens
            .Where(t => t.FamilyId == familyId && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, revokedAt), ct);
    }

    public async Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default)
    {
        var revokedAt = DateTimeOffset.UtcNow;
        await context.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, revokedAt), ct);
    }

    public async Task AddAsync(RefreshToken token, CancellationToken ct = default)
        => await context.RefreshTokens.AddAsync(token, ct);

    public void Update(RefreshToken token)
        => context.RefreshTokens.Update(token);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await context.SaveChangesAsync(ct);
}
