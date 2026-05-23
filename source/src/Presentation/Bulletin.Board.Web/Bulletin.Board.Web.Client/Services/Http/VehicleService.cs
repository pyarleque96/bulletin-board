using System.Net;
using System.Net.Http.Json;
using Bulletin.Board.Web.Client.Models;
using Bulletin.Board.Web.Client.Services.Common;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Web.Client.Services.Http;

/// <summary>
/// Typed HTTP client implementation for <see cref="IVehicleService"/>.
/// </summary>
public sealed class VehicleService : IVehicleService
{
    private readonly HttpClient _http;
    private readonly ILogger<VehicleService> _logger;

    public VehicleService(HttpClient http, ILogger<VehicleService> logger)
    {
        _http   = http;
        _logger = logger;
    }

    public Task<ApiResult<PagedResultDto<OwnerVehicleDto>>> GetByListingAsync(
        Guid              listingId,
        int               page     = 1,
        int               pageSize = 20,
        CancellationToken ct       = default)
        => GetAsync<PagedResultDto<OwnerVehicleDto>>(
            $"api/v1/listings/{listingId}/vehicles?page={page}&pageSize={pageSize}", ct);

    public async Task<ApiResult<Guid>> CreateAsync(
        Guid                  listingId,
        CreateVehicleRequest  request,
        CancellationToken     ct = default)
    {
        try
        {
            var response = await _http.PostAsJsonAsync(
                $"api/v1/listings/{listingId}/vehicles", request, ct);

            if (response.IsSuccessStatusCode)
            {
                var payload = await response.Content.ReadFromJsonAsync<IdResponse>(ct);
                return payload?.Id is Guid id
                    ? ApiResult<Guid>.Ok(id)
                    : ApiResult<Guid>.FromStatusCode(HttpStatusCode.NoContent);
            }

            var raw = await TryReadProblemDetailAsync(response, ct);
            return ApiResult<Guid>.FromStatusCode(response.StatusCode, raw);
        }
        catch (Exception ex) when (ex is TaskCanceledException or HttpRequestException)
        {
            _logger.LogError(ex, "Create vehicle failed for listing {ListingId}", listingId);
            return ApiResult<Guid>.NetworkError(ex.Message);
        }
    }

    public Task<ApiResult<bool>> UpdateAsync(
        Guid                  vehicleId,
        UpdateVehicleRequest  request,
        CancellationToken     ct = default)
        => MutateAsync(HttpMethod.Put, $"api/v1/vehicles/{vehicleId}", request, ct);

    public Task<ApiResult<bool>> SetActiveAsync(
        Guid              vehicleId,
        bool              isActive,
        CancellationToken ct = default)
        => MutateAsync(HttpMethod.Patch, $"api/v1/vehicles/{vehicleId}/active",
            new { IsActive = isActive }, ct);

    public Task<ApiResult<bool>> DeleteAsync(
        Guid              vehicleId,
        CancellationToken ct = default)
        => MutateAsync(HttpMethod.Delete, $"api/v1/vehicles/{vehicleId}", ct);

    // ── Internals ──────────────────────────────────────────────────────────────

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

    private Task<ApiResult<bool>> MutateAsync<TBody>(
        HttpMethod method, string url, TBody body, CancellationToken ct)
        => MutateInternalAsync(method, url, JsonContent.Create(body), ct);

    private Task<ApiResult<bool>> MutateAsync(HttpMethod method, string url, CancellationToken ct)
        => MutateInternalAsync(method, url, content: null, ct);

    private async Task<ApiResult<bool>> MutateInternalAsync(
        HttpMethod method, string url, HttpContent? content, CancellationToken ct)
    {
        try
        {
            using var request = new HttpRequestMessage(method, url);
            if (content is not null) request.Content = content;

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

    private record IdResponse(Guid Id);
}
