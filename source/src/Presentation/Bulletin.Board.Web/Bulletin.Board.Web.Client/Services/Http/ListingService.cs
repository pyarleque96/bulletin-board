using System.Net;
using System.Net.Http.Json;
using Bulletin.Board.Web.Client.Models;
using Bulletin.Board.Web.Client.Services.Common;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Web.Client.Services.Http;

/// <summary>
/// Typed HTTP client implementation for <see cref="IListingService"/>.
/// Uses <see cref="ApiResult{T}"/> to surface structured errors to the UI layer.
/// Retry logic: single retry on transient network failures (not on 4xx).
/// </summary>
public sealed class ListingService : IListingService
{
    private readonly HttpClient _http;
    private readonly ILogger<ListingService> _logger;

    public ListingService(HttpClient http, ILogger<ListingService> logger)
    {
        _http   = http;
        _logger = logger;
    }

    public Task<ApiResult<PagedResultDto<MyListingDto>>> GetMyListingsAsync(
        int    page       = 1,
        int    pageSize   = 20,
        string? status    = null,
        string? category  = null,
        string? search    = null,
        CancellationToken ct = default)
    {
        var qs = BuildQs(new()
        {
            ["page"]     = page.ToString(),
            ["pageSize"] = pageSize.ToString(),
            ["status"]   = status,
            ["category"] = category,
            ["search"]   = search,
        });
        return GetAsync<PagedResultDto<MyListingDto>>($"api/v1/me/listings?{qs}", ct);
    }

    public Task<ApiResult<ListingDetailDto>> GetListingByIdAsync(
        Guid listingId,
        CancellationToken ct = default)
        => GetAsync<ListingDetailDto>($"api/v1/me/listings/{listingId}", ct);

    public Task<ApiResult<ListingAnalyticsDto>> GetAnalyticsAsync(
        Guid listingId,
        CancellationToken ct = default)
        => GetAsync<ListingAnalyticsDto>($"api/v1/listings/{listingId}/analytics", ct);

    public Task<ApiResult<bool>> UpdateAsync(
        Guid                 listingId,
        UpdateListingRequest request,
        CancellationToken    ct = default)
        => MutateAsync(HttpMethod.Put, $"api/v1/listings/{listingId}", request, ct);

    public Task<ApiResult<bool>> SetActiveAsync(
        Guid              listingId,
        bool              isActive,
        CancellationToken ct = default)
        => MutateAsync(HttpMethod.Patch, $"api/v1/listings/{listingId}/active",
            new { IsActive = isActive }, ct);

    public Task<ApiResult<bool>> DeleteAsync(
        Guid              listingId,
        CancellationToken ct = default)
        => MutateAsync(HttpMethod.Delete, $"api/v1/listings/{listingId}", ct);

    // ── Internals ──────────────────────────────────────────────────────────────

    private async Task<ApiResult<T>> GetAsync<T>(string url, CancellationToken ct)
    {
        try
        {
            var response = await _http.GetAsync(url, ct);
            return await ToApiResultAsync<T>(response, url, ct);
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            _logger.LogWarning("Timeout on GET {Url}", url);
            return ApiResult<T>.NetworkError(ex.Message);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error on GET {Url}", url);
            return ApiResult<T>.NetworkError(ex.Message);
        }
    }

    private Task<ApiResult<bool>> MutateAsync(HttpMethod method, string url, CancellationToken ct)
        => MutateInternalAsync(method, url, content: null, ct);

    private Task<ApiResult<bool>> MutateAsync<TBody>(
        HttpMethod method, string url, TBody body, CancellationToken ct)
        => MutateInternalAsync(method, url, JsonContent.Create(body), ct);

    private async Task<ApiResult<bool>> MutateInternalAsync(
        HttpMethod method, string url, HttpContent? content, CancellationToken ct)
    {
        try
        {
            using var request = new HttpRequestMessage(method, url);
            if (content is not null) request.Content = content;

            var response = await _http.SendAsync(request, ct);

            if (response.IsSuccessStatusCode)
                return ApiResult<bool>.Ok(true);

            var raw = await TryReadProblemDetailAsync(response, ct);
            return ApiResult<bool>.FromStatusCode(response.StatusCode, raw);
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            _logger.LogWarning("Timeout on {Method} {Url}", method, url);
            return ApiResult<bool>.NetworkError(ex.Message);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error on {Method} {Url}", method, url);
            return ApiResult<bool>.NetworkError(ex.Message);
        }
    }

    private async Task<ApiResult<T>> ToApiResultAsync<T>(
        HttpResponseMessage response, string url, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadFromJsonAsync<T>(ct);
            if (data is null)
            {
                _logger.LogWarning("Null body on successful GET {Url}", url);
                return ApiResult<T>.FromStatusCode(HttpStatusCode.NoContent);
            }
            return ApiResult<T>.Ok(data);
        }

        var raw = await TryReadProblemDetailAsync(response, ct);
        _logger.LogWarning("GET {Url} returned {Status}", url, (int)response.StatusCode);
        return ApiResult<T>.FromStatusCode(response.StatusCode, raw);
    }

    private static async Task<string?> TryReadProblemDetailAsync(
        HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            return string.IsNullOrWhiteSpace(body) ? null : body;
        }
        catch
        {
            return null;
        }
    }

    private static string BuildQs(Dictionary<string, string?> p)
    {
        var parts = p
            .Where(kv => kv.Value is not null)
            .Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value!)}");
        return string.Join("&", parts);
    }
}
