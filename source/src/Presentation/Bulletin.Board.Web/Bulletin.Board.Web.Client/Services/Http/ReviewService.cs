using System.Net;
using System.Net.Http.Json;
using Bulletin.Board.Web.Client.Models;
using Bulletin.Board.Web.Client.Services.Common;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Web.Client.Services.Http;

/// <summary>
/// Typed HTTP client implementation for <see cref="IReviewService"/>.
/// </summary>
public sealed class ReviewService : IReviewService
{
    private readonly HttpClient _http;
    private readonly ILogger<ReviewService> _logger;

    public ReviewService(HttpClient http, ILogger<ReviewService> logger)
    {
        _http   = http;
        _logger = logger;
    }

    public Task<ApiResult<PagedResultDto<ListingReviewDto>>> GetByListingAsync(
        Guid              listingId,
        int               page     = 1,
        int               pageSize = 20,
        CancellationToken ct       = default)
        => GetAsync<PagedResultDto<ListingReviewDto>>(
            $"api/v1/listings/{listingId}/reviews?page={page}&pageSize={pageSize}", ct);

    // ── Internal ───────────────────────────────────────────────────────────────

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

    private static async Task<string?> TryReadProblemDetailAsync(
        HttpResponseMessage response, CancellationToken ct)
    {
        try { return await response.Content.ReadAsStringAsync(ct); }
        catch { return null; }
    }
}
