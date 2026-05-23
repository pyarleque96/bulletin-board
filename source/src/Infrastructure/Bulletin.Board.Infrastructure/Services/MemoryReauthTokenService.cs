using Bulletin.Board.Domain.Interfaces.Services;
using Microsoft.Extensions.Caching.Memory;

namespace Bulletin.Board.Infrastructure.Services;

public sealed class MemoryReauthTokenService(IMemoryCache cache) : IReauthTokenService
{
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromMinutes(5);

    public string IssueToken(Guid userId)
    {
        var token = Guid.NewGuid().ToString("N");
        cache.Set(Key(token), userId, TokenLifetime);
        return token;
    }

    public bool ValidateAndConsume(string token, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(token)) return false;

        var key = Key(token);
        if (!cache.TryGetValue<Guid>(key, out var storedUserId)) return false;
        if (storedUserId != userId) return false;

        cache.Remove(key);
        return true;
    }

    private static string Key(string token) => $"reauth:{token}";
}
