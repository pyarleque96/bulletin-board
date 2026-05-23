using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Web.Client.Services.Http;

/// <summary>
/// Fire-and-forget implementation of <see cref="IContactLogService"/>.
/// Per Regla #8: 2-second timeout, silent failure. The WhatsApp link MUST open
/// regardless of whether this call succeeds.
/// </summary>
public sealed class ContactLogService : IContactLogService
{
    private readonly HttpClient _http;
    private readonly ILogger<ContactLogService> _logger;

    // Hard 2-second timeout as per spec — never blocks the UX flow.
    private static readonly TimeSpan LogTimeout = TimeSpan.FromSeconds(2);

    public ContactLogService(HttpClient http, ILogger<ContactLogService> logger)
    {
        _http   = http;
        _logger = logger;
    }

    /// <inheritdoc/>
    public void LogContact(Guid reservationId, string channel = "WhatsApp")
    {
        // Intentionally not awaited — fire and forget.
        _ = FireAndForgetAsync(reservationId, channel);
    }

    private async Task FireAndForgetAsync(Guid reservationId, string channel)
    {
        using var cts = new CancellationTokenSource(LogTimeout);
        try
        {
            var body = new { Channel = channel };
            var response = await _http.PostAsJsonAsync(
                $"api/v1/reservations/{reservationId}/contact-log", body, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "ContactLog POST returned {Status} for reservation {ReservationId}",
                    (int)response.StatusCode, reservationId);
            }
        }
        catch (TaskCanceledException)
        {
            // Timeout — expected, silent.
            _logger.LogWarning(
                "ContactLog POST timed out (>2s) for reservation {ReservationId}", reservationId);
        }
        catch (Exception ex)
        {
            // Network error — silent, never rethrow.
            _logger.LogWarning(ex,
                "ContactLog POST failed silently for reservation {ReservationId}", reservationId);
        }
    }
}
