using System.Net.Http.Json;
using Bulletin.Board.Web.Client.Models;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Web.Client.Services;

/// <summary>
/// Typed HTTP client for /api/v1/categories.
///
/// Public read (GetAllAsync):   returns empty array on error — UI degrades gracefully.
/// Admin mutations (Create/Update/Delete): throw on failure — callers surface the error.
///
/// All requests go through the same-origin BFF; auth is via HttpOnly cookie.
/// Admin endpoints return 403 if the user lacks the Admin role.
/// </summary>
public sealed class CategoriesApiService : ICategoriesService
{
    private readonly HttpClient _http;
    private readonly ILogger<CategoriesApiService> _logger;

    // Simple in-memory cache to avoid re-fetching on every navigation.
    // Invalidated by any successful mutation.
    private CategoryDto[]? _cache;
    private CategoryDto[]? _cacheWithCount;

    public CategoriesApiService(HttpClient http, ILogger<CategoriesApiService> logger)
    {
        _http   = http;
        _logger = logger;
    }

    // ── Public ───────────────────────────────────────────────────────────

    public async Task<CategoryDto[]> GetAllAsync(bool includeCount = false, CancellationToken ct = default)
    {
        if (includeCount)
        {
            if (_cacheWithCount is not null)
                return _cacheWithCount;

            try
            {
                var result = await _http.GetFromJsonAsync<CategoryDto[]>("api/v1/categories?includeCount=true", ct);
                _cacheWithCount = result ?? [];
                return _cacheWithCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch categories with counts");
                return [];
            }
        }
        else
        {
            if (_cache is not null)
                return _cache;

            try
            {
                var result = await _http.GetFromJsonAsync<CategoryDto[]>("api/v1/categories", ct);
                _cache = result ?? [];
                return _cache;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch categories");
                return [];
            }
        }
    }

    // ── Admin ────────────────────────────────────────────────────────────

    public async Task<CategoryAdminDto[]> GetAllAdminAsync(CancellationToken ct = default)
    {
        // Admin endpoint returns ALL categories including inactive ones.
        // Query param "includeInactive=true" is used when the backend supports it;
        // if the endpoint falls back to the public one the page will work with active
        // categories only — acceptable degradation.
        var result = await _http.GetFromJsonAsync<CategoryAdminDto[]>(
            "api/v1/admin/categories", ct);

        return result ?? [];
    }

    public async Task<CategoryAdminDto> CreateAsync(
        CategoryCreateModel model,
        CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/v1/categories", model, ct);
        response.EnsureSuccessStatusCode();

        var dto = await response.Content.ReadFromJsonAsync<CategoryAdminDto>(ct)
            ?? throw new InvalidOperationException("Server returned null after category creation.");

        InvalidateCache();
        return dto;
    }

    public async Task UpdateAsync(
        Guid id,
        CategoryUpdateModel model,
        CancellationToken ct = default)
    {
        var response = await _http.PutAsJsonAsync($"api/v1/categories/{id}", model, ct);
        response.EnsureSuccessStatusCode();
        InvalidateCache();
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _http.DeleteAsync($"api/v1/categories/{id}", ct);
        response.EnsureSuccessStatusCode();
        InvalidateCache();
    }

    // ── Internal helpers ─────────────────────────────────────────────────

    /// <summary>Clears all public-facing caches so the next GetAllAsync call re-fetches.</summary>
    private void InvalidateCache() { _cache = null; _cacheWithCount = null; }
}
