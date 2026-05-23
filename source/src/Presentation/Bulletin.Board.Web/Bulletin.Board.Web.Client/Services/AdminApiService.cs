using System.Net.Http.Json;
using System.Text;
using Bulletin.Board.Web.Client.Models;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Web.Client.Services;

/// <summary>
/// Typed HTTP client for /bff/api/v1/admin/*.
///
/// All requests go to the same-origin BFF, which proxies to the upstream API
/// after injecting the JWT from the auth cookie. The browser never sees the JWT.
///
/// Read operations return null / empty PagedResult on failure; mutations throw.
/// </summary>
public sealed class AdminApiService : IAdminService
{
    private readonly HttpClient _http;
    private readonly ILogger<AdminApiService> _logger;

    public AdminApiService(HttpClient http, ILogger<AdminApiService> logger)
    {
        _http = http;
        _logger = logger;
    }

    // ── Stats ────────────────────────────────────────────────

    public async Task<AdminStatsDto?> GetStatsAsync(CancellationToken ct = default)
    {
        try
        {
            return await _http.GetFromJsonAsync<AdminStatsDto>("api/v1/admin/stats", ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch admin stats");
            return null;
        }
    }

    // ── Listings ─────────────────────────────────────────────

    public async Task<PagedResult<AdminListingDto>> GetListingsAsync(
        string? status,
        string? tier,
        Guid? categoryId,
        int page,
        int pageSize,
        Guid? providerId = null,
        CancellationToken ct = default)
    {
        try
        {
            var url = BuildListingsUrl(status, tier, categoryId, providerId, page, pageSize);
            var result = await _http.GetFromJsonAsync<PagedResult<AdminListingDto>>(url, ct);
            return result ?? EmptyPaged<AdminListingDto>(page, pageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch admin listings");
            return EmptyPaged<AdminListingDto>(page, pageSize);
        }
    }

    public async Task ApproveListingAsync(Guid id, string? feedbackEn = null, string? feedbackEs = null, CancellationToken ct = default)
    {
        var body = new ApproveListingRequest(feedbackEn, feedbackEs);
        var response = await _http.PostAsJsonAsync($"api/v1/admin/listings/{id}/approve", body, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task RejectListingAsync(Guid id, string reason, CancellationToken ct = default)
    {
        var body = new RejectListingRequest(reason);
        var response = await _http.PostAsJsonAsync($"api/v1/admin/listings/{id}/reject", body, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task RequestChangesAsync(Guid id, string feedback, CancellationToken ct = default)
    {
        var body = new RequestChangesRequest(feedback);
        var response = await _http.PostAsJsonAsync($"api/v1/admin/listings/{id}/request-changes", body, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteListingAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _http.DeleteAsync($"api/v1/admin/listings/{id}", ct);
        response.EnsureSuccessStatusCode();
    }

    // ── Ratings ──────────────────────────────────────────────

    public async Task<PagedResult<AdminRatingDto>> GetRatingsAsync(
        string? status,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        try
        {
            var url = BuildRatingsUrl(status, page, pageSize);
            var result = await _http.GetFromJsonAsync<PagedResult<AdminRatingDto>>(url, ct);
            return result ?? EmptyPaged<AdminRatingDto>(page, pageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch admin ratings");
            return EmptyPaged<AdminRatingDto>(page, pageSize);
        }
    }

    public async Task ApproveRatingAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _http.PostAsync($"api/v1/admin/ratings/{id}/approve", null, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task RejectRatingAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _http.PostAsync($"api/v1/admin/ratings/{id}/reject", null, ct);
        response.EnsureSuccessStatusCode();
    }

    // ── Providers ────────────────────────────────────────────

    public async Task<PagedResult<AdminProviderDto>> GetProvidersAsync(
        string? tier,
        string? verificationStatus,
        int page,
        int pageSize,
        string? sortBy = null,
        string? sortDir = null,
        CancellationToken ct = default)
    {
        try
        {
            var url = BuildProvidersUrl(tier, verificationStatus, page, pageSize, sortBy, sortDir);
            var result = await _http.GetFromJsonAsync<PagedResult<AdminProviderDto>>(url, ct);
            return result ?? EmptyPaged<AdminProviderDto>(page, pageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch admin providers");
            return EmptyPaged<AdminProviderDto>(page, pageSize);
        }
    }

    public async Task ChangeProviderTierAsync(Guid id, string tier, CancellationToken ct = default)
    {
        var body = new ChangeProviderTierRequest(tier);
        var response = await _http.PutAsJsonAsync($"api/v1/admin/providers/{id}/tier", body, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task SetProviderHierarchyAsync(Guid id, int hierarchy, CancellationToken ct = default)
    {
        var body = new SetProviderHierarchyRequest(hierarchy);
        var response = await _http.PutAsJsonAsync($"api/v1/admin/providers/{id}/hierarchy", body, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task<RecomputeTrendingResponse?> RecomputeTrendingAsync(CancellationToken ct = default)
    {
        var response = await _http.PostAsync("api/v1/admin/trending/recompute", content: null, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<RecomputeTrendingResponse>(cancellationToken: ct);
    }

    // ── VIP toggle ───────────────────────────────────────────

    public async Task<ReauthResponse?> ReauthAsync(string password, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/v1/admin/auth/reauth", new ReauthRequest(password), ct);
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ReauthResponse>(cancellationToken: ct);
    }

    public Task GrantProviderVipAsync(Guid id, GrantVipRequest body, string reauthToken, CancellationToken ct = default)
        => SendVipAsync(HttpMethod.Post, $"api/v1/admin/providers/{id}/vip", body, reauthToken, ct);

    public Task RevokeProviderVipAsync(Guid id, RevokeVipRequest body, string reauthToken, CancellationToken ct = default)
        => SendVipAsync(HttpMethod.Delete, $"api/v1/admin/providers/{id}/vip", body, reauthToken, ct);

    public Task GrantListingVipAsync(Guid id, GrantVipRequest body, string reauthToken, CancellationToken ct = default)
        => SendVipAsync(HttpMethod.Post, $"api/v1/admin/listings/{id}/vip", body, reauthToken, ct);

    public Task RevokeListingVipAsync(Guid id, RevokeVipRequest body, string reauthToken, CancellationToken ct = default)
        => SendVipAsync(HttpMethod.Delete, $"api/v1/admin/listings/{id}/vip", body, reauthToken, ct);

    public async Task<IReadOnlyList<VipChangeLogDto>> GetVipLogsAsync(
        VipEntityKind entityType, Guid entityId, int page = 1, int pageSize = 50, CancellationToken ct = default)
    {
        try
        {
            var url = $"api/v1/admin/vip-logs?entityType={entityType}&entityId={entityId}&page={page}&pageSize={pageSize}";
            var rows = await _http.GetFromJsonAsync<List<VipChangeLogDto>>(url, ct);
            return rows ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch VIP logs");
            return [];
        }
    }

    private async Task SendVipAsync(HttpMethod method, string url, object body, string reauthToken, CancellationToken ct)
    {
        var req = new HttpRequestMessage(method, url)
        {
            Content = JsonContent.Create(body)
        };
        req.Headers.Add("X-Reauth-Token", reauthToken);
        var response = await _http.SendAsync(req, ct);
        response.EnsureSuccessStatusCode();
    }

    // ── URL builders ─────────────────────────────────────────

    private static string BuildListingsUrl(string? status, string? tier, Guid? categoryId, Guid? providerId, int page, int pageSize)
    {
        var sb = new StringBuilder("api/v1/admin/listings?");
        sb.Append($"page={page}&pageSize={pageSize}");
        if (!string.IsNullOrEmpty(status)) sb.Append($"&status={Uri.EscapeDataString(status)}");
        if (!string.IsNullOrEmpty(tier)) sb.Append($"&tier={Uri.EscapeDataString(tier)}");
        if (categoryId.HasValue) sb.Append($"&categoryId={categoryId.Value}");
        if (providerId.HasValue) sb.Append($"&providerId={providerId.Value}");
        return sb.ToString();
    }

    private static string BuildRatingsUrl(string? status, int page, int pageSize)
    {
        var sb = new StringBuilder("api/v1/admin/ratings?");
        sb.Append($"page={page}&pageSize={pageSize}");
        if (!string.IsNullOrEmpty(status)) sb.Append($"&status={Uri.EscapeDataString(status)}");
        return sb.ToString();
    }

    private static string BuildProvidersUrl(
        string? tier,
        string? verificationStatus,
        int page,
        int pageSize,
        string? sortBy,
        string? sortDir)
    {
        var sb = new StringBuilder("api/v1/admin/providers?");
        sb.Append($"page={page}&pageSize={pageSize}");
        if (!string.IsNullOrEmpty(tier)) sb.Append($"&tier={Uri.EscapeDataString(tier)}");
        if (!string.IsNullOrEmpty(verificationStatus)) sb.Append($"&verificationStatus={Uri.EscapeDataString(verificationStatus)}");
        if (!string.IsNullOrEmpty(sortBy)) sb.Append($"&sortBy={Uri.EscapeDataString(sortBy)}");
        if (!string.IsNullOrEmpty(sortDir)) sb.Append($"&sortDir={Uri.EscapeDataString(sortDir)}");
        return sb.ToString();
    }

    private static PagedResult<T> EmptyPaged<T>(int page, int pageSize) =>
        new([], 0, page, pageSize);
}
