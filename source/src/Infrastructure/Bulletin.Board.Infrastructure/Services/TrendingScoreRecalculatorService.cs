using Bulletin.Board.Domain.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Infrastructure.Services;

/// <summary>
/// Hourly background driver around <see cref="ITrendingScoreCalculator"/>. First run fires
/// shortly after startup so a freshly-deployed environment does not sit on zero scores
/// until the first interval ticks.
/// </summary>
public sealed class TrendingScoreRecalculatorService(
    IServiceProvider services,
    ILogger<TrendingScoreRecalculatorService> logger) : BackgroundService
{
    private static readonly TimeSpan RunInterval = TimeSpan.FromHours(1);
    private static readonly TimeSpan InitialDelay = TimeSpan.FromSeconds(10);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("TrendingScoreRecalculator started. Will run every {Minutes}m.", RunInterval.TotalMinutes);

        try
        {
            await Task.Delay(InitialDelay, stoppingToken);
            await RunRecalcAsync(stoppingToken);
        }
        catch (OperationCanceledException) { return; }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(RunInterval, stoppingToken);
            }
            catch (OperationCanceledException) { break; }

            await RunRecalcAsync(stoppingToken);
        }

        logger.LogInformation("TrendingScoreRecalculator stopping.");
    }

    private async Task RunRecalcAsync(CancellationToken ct)
    {
        try
        {
            using var scope = services.CreateScope();
            var calculator = scope.ServiceProvider.GetRequiredService<ITrendingScoreCalculator>();

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var affected = await calculator.RecalculateAsync(ct);
            sw.Stop();

            logger.LogInformation(
                "TrendingScore recalc: {Count} listings updated in {Ms}ms.",
                affected, sw.ElapsedMilliseconds);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "TrendingScore recalc failed. Will retry in {Minutes}m.", RunInterval.TotalMinutes);
        }
    }
}
