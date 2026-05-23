using System.Net;
using System.Net.Http.Json;
using Bulletin.Board.Web.Client.Models;   // ReservationDecisionRequest, ReservationDto
using Bulletin.Board.Web.Client.Services.Common;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Web.Client.Services.Http;

/// <summary>
/// Typed HTTP client implementation for <see cref="IReservationService"/>.
/// </summary>
public sealed class ReservationService : IReservationService
{
    private readonly HttpClient _http;
    private readonly ILogger<ReservationService> _logger;

    public ReservationService(HttpClient http, ILogger<ReservationService> logger)
    {
        _http   = http;
        _logger = logger;
    }

    public Task<ApiResult<PagedResultDto<ReservationDto>>> GetByListingAsync(
        Guid              listingId,
        string?           status   = null,
        int               page     = 1,
        int               pageSize = 20,
        CancellationToken ct       = default)
    {
        var qs = BuildQs(new()
        {
            ["status"]   = status,
            ["page"]     = page.ToString(),
            ["pageSize"] = pageSize.ToString(),
        });
        return GetAsync<PagedResultDto<ReservationDto>>(
            $"api/v1/listings/{listingId}/reservations?{qs}", ct);
    }

    public Task<ApiResult<bool>> AcceptAsync(
        Guid              reservationId,
        string?           note = null,
        CancellationToken ct   = default)
        => PostDecisionAsync(reservationId, "accept", note, ct);

    public Task<ApiResult<bool>> RejectAsync(
        Guid              reservationId,
        string?           note = null,
        CancellationToken ct   = default)
        => PostDecisionAsync(reservationId, "reject", note, ct);

    public Task<ApiResult<bool>> CancelAsync(
        Guid              reservationId,
        string?           note = null,
        CancellationToken ct   = default)
        => PostDecisionAsync(reservationId, "cancel", note, ct);

    public Task<ApiResult<bool>> MarkCompletedAsync(
        Guid              reservationId,
        CancellationToken ct = default)
        => PostDecisionAsync(reservationId, "mark-completed", note: null, ct);

    // ── Internals ──────────────────────────────────────────────────────────────

    private Task<ApiResult<bool>> PostDecisionAsync(
        Guid reservationId, string action, string? note, CancellationToken ct)
    {
        var body = new ReservationDecisionRequest(note);
        return MutateAsync(HttpMethod.Post,
            $"api/v1/reservations/{reservationId}/{action}", body, ct);
    }

    private async Task<ApiResult<T>> GetAsync<T>(string url, CancellationToken ct)
    {
        try
        {
            var response = await _http.GetAsync(url, ct);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<T>(ct);
                if (data is null) return ApiResult<T>.FromStatusCode(HttpStatusCode.NoContent);
                return ApiResult<T>.Ok(data);
            }

            var raw = await TryReadProblemDetailAsync(response, ct);
            return ApiResult<T>.FromStatusCode(response.StatusCode, raw);
        }
        catch (Exception ex) when (ex is TaskCanceledException or HttpRequestException)
        {
            _logger.LogError(ex, "GET {Url} failed", url);
            return ApiResult<T>.NetworkError(ex.Message);
        }
    }

    private async Task<ApiResult<bool>> MutateAsync<TBody>(
        HttpMethod method, string url, TBody body, CancellationToken ct)
    {
        try
        {
            using var request = new HttpRequestMessage(method, url)
            {
                Content = JsonContent.Create(body)
            };
            var response = await _http.SendAsync(request, ct);

            if (response.IsSuccessStatusCode) return ApiResult<bool>.Ok(true);
            var raw = await TryReadProblemDetailAsync(response, ct);
            return ApiResult<bool>.FromStatusCode(response.StatusCode, raw);
        }
        catch (Exception ex) when (ex is TaskCanceledException or HttpRequestException)
        {
            _logger.LogError(ex, "{Method} {Url} failed", method, url);
            return ApiResult<bool>.NetworkError(ex.Message);
        }
    }

    private static async Task<string?> TryReadProblemDetailAsync(
        HttpResponseMessage response, CancellationToken ct)
    {
        try { return await response.Content.ReadAsStringAsync(ct); }
        catch { return null; }
    }

    private static string BuildQs(Dictionary<string, string?> p)
    {
        var parts = p
            .Where(kv => kv.Value is not null)
            .Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value!)}");
        return string.Join("&", parts);
    }
}
