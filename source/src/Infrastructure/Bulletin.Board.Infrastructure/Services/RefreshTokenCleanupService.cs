using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Bulletin.Board.Infrastructure.Persistence;

namespace Bulletin.Board.Infrastructure.Services;

/// <summary>
/// Background service que limpia tokens de refresco expirados Y revocados cada 24 horas.
/// Solo elimina tokens que cumplan AMBAS condiciones para preservar el historial de sesiones
/// activas (revocados pero no expirados todavía son detectables como reuse).
/// </summary>
public sealed class RefreshTokenCleanupService(
    IServiceProvider services,
    ILogger<RefreshTokenCleanupService> logger) : BackgroundService
{
    private static readonly TimeSpan RunInterval = TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("RefreshTokenCleanupService started. Will run every {Hours}h.", RunInterval.TotalHours);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(RunInterval, stoppingToken);

            if (stoppingToken.IsCancellationRequested)
                break;

            await RunCleanupAsync(stoppingToken);
        }

        logger.LogInformation("RefreshTokenCleanupService stopping.");
    }

    private async Task RunCleanupAsync(CancellationToken ct)
    {
        try
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var cutoff = DateTimeOffset.UtcNow;

            var deleted = await db.RefreshTokens
                .Where(t => t.ExpiresAt < cutoff && t.RevokedAt != null)
                .ExecuteDeleteAsync(ct);

            if (deleted > 0)
                logger.LogInformation("RefreshToken cleanup: deleted {Count} expired+revoked tokens.", deleted);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Error during RefreshToken cleanup. Will retry in {Hours}h.", RunInterval.TotalHours);
        }
    }
}
