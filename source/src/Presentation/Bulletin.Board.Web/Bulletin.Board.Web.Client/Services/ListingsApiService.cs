using System.Net;
using System.Net.Http.Json;
using Bulletin.Board.Web.Client.Models;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Web.Client.Services;

/// <summary>
/// Typed HTTP client for the Listings API endpoints (/api/v1/listings).
/// Handles 404 → null, 401 → null (auth required), 5xx → log + throw.
/// </summary>
public sealed class ListingsApiService : IListingsService
{
    private readonly HttpClient _http;
    private readonly ILogger<ListingsApiService> _logger;

    public ListingsApiService(HttpClient http, ILogger<ListingsApiService> logger)
    {
        _http   = http;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ListingCardDto>?> GetTopAsync(int count = 10, CancellationToken ct = default)
    {
        return await SafeGetAsync<IReadOnlyList<ListingCardDto>>($"api/v1/listings/top?count={count}", ct);
    }

    public async Task<PagedResult<ListingCardDto>?> SearchAsync(
        string?  category    = null,
        string?  tier        = null,
        decimal? minRating   = null,
        string?  language    = null,
        int      page        = 1,
        int      pageSize    = 12,
        CancellationToken ct = default)
    {
        var query = BuildQueryString(new Dictionary<string, string?>
        {
            ["category"]  = category,
            ["tier"]      = tier,
            ["minRating"] = minRating?.ToString("F1"),
            ["language"]  = language,
            ["page"]      = page.ToString(),
            ["pageSize"]  = pageSize.ToString(),
        });

        return await SafeGetAsync<PagedResult<ListingCardDto>>($"api/v1/listings?{query}", ct);
    }

    public async Task<ListingDetailDto?> GetDetailAsync(Guid id, CancellationToken ct = default)
    {
        return await SafeGetAsync<ListingDetailDto>($"api/v1/listings/{id}", ct);
    }

    public async Task<ListingDetailDto?> GetDetailBySlugAsync(string slug, CancellationToken ct = default)
    {
        // Slug travels via path segment; escape user input to keep route parsing safe.
        var encoded = Uri.EscapeDataString(slug);
        return await SafeGetAsync<ListingDetailDto>($"api/v1/listings/by-slug/{encoded}", ct);
    }

    // ─── Vehicle management (owner) ────────────────────────────────────────────

    public Task<IReadOnlyList<OwnerVehicleDto>?> GetOwnerVehiclesAsync(Guid listingId, CancellationToken ct = default)
        => SafeGetAsync<IReadOnlyList<OwnerVehicleDto>>($"api/v1/listings/{listingId}/vehicles", ct);

    public async Task<Guid?> CreateVehicleAsync(Guid listingId, CreateVehicleRequest request, CancellationToken ct = default)
    {
        var (id, _) = await SafePostForIdAsync($"api/v1/listings/{listingId}/vehicles", request, ct);
        return id;
    }

    public Task<bool> UpdateVehicleAsync(Guid vehicleId, UpdateVehicleRequest request, CancellationToken ct = default)
        => SafeMutateAsync(HttpMethod.Put, $"api/v1/vehicles/{vehicleId}", request, ct);

    public Task<bool> DeleteVehicleAsync(Guid vehicleId, CancellationToken ct = default)
        => SafeMutateAsync(HttpMethod.Delete, $"api/v1/vehicles/{vehicleId}", ct);

    public Task<bool> SetVehicleActiveAsync(Guid vehicleId, bool isActive, CancellationToken ct = default)
        => SafeMutateAsync(HttpMethod.Patch, $"api/v1/vehicles/{vehicleId}/active", new { IsActive = isActive }, ct);

    // ─── Vehicle photos ────────────────────────────────────────────────────────

    public async Task<Guid?> AddVehiclePhotoAsync(Guid vehicleId, AddVehiclePhotoRequest request, CancellationToken ct = default)
    {
        var (id, _) = await SafePostForIdAsync($"api/v1/vehicles/{vehicleId}/photos", request, ct);
        return id;
    }

    public async Task<Guid?> UploadVehiclePhotoAsync(
        Guid vehicleId, Stream content, string fileName, string contentType,
        int displayOrder, bool isPrimary, CancellationToken ct = default)
    {
        try
        {
            using var form = new MultipartFormDataContent();
            using var fileContent = new StreamContent(content);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            form.Add(fileContent, "file", fileName);
            form.Add(new StringContent(displayOrder.ToString()), "displayOrder");
            form.Add(new StringContent(isPrimary.ToString().ToLowerInvariant()), "isPrimary");

            var response = await _http.PostAsync($"api/v1/vehicles/{vehicleId}/photos/upload", form, ct);

            if (response.StatusCode is HttpStatusCode.Unauthorized
                or HttpStatusCode.Forbidden
                or HttpStatusCode.NotFound
                or HttpStatusCode.BadRequest
                or HttpStatusCode.RequestEntityTooLarge)
                return null;

            response.EnsureSuccessStatusCode();
            var payload = await response.Content.ReadFromJsonAsync<UploadResponse>(ct);
            return payload?.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Vehicle photo upload failed for vehicle {VehicleId}", vehicleId);
            throw;
        }
    }

    private record UploadResponse(Guid Id, string Url);

    public Task<bool> DeleteVehiclePhotoAsync(Guid photoId, CancellationToken ct = default)
        => SafeMutateAsync(HttpMethod.Delete, $"api/v1/vehicle-photos/{photoId}", ct);

    public Task<bool> SetVehiclePhotoPrimaryAsync(Guid photoId, CancellationToken ct = default)
        => SafeMutateAsync(HttpMethod.Patch, $"api/v1/vehicle-photos/{photoId}/primary", ct);

    // ─── Availability ──────────────────────────────────────────────────────────

    public Task<VehicleAvailabilityDto?> GetVehicleAvailabilityAsync(
        Guid vehicleId, DateOnly from, DateOnly to, CancellationToken ct = default)
        => SafeGetAsync<VehicleAvailabilityDto>(
            $"api/v1/vehicles/{vehicleId}/availability?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}", ct);

    public async Task<Guid?> AddVehicleUnavailabilityAsync(
        Guid vehicleId, AddUnavailabilityRequest request, CancellationToken ct = default)
    {
        var (id, status) = await SafePostForIdAsync($"api/v1/vehicles/{vehicleId}/unavailability", request, ct);
        return status == HttpStatusCode.Conflict ? null : id;
    }

    public Task<bool> DeleteVehicleUnavailabilityAsync(Guid unavailabilityId, CancellationToken ct = default)
        => SafeMutateAsync(HttpMethod.Delete, $"api/v1/vehicle-unavailability/{unavailabilityId}", ct);

    // ─── Reservations ─────────────────────────────────────────────────────────

    public async Task<Guid?> CreateReservationAsync(
        Guid vehicleId, CreateReservationRequest request, CancellationToken ct = default)
    {
        var (id, _) = await SafePostForIdAsync($"api/v1/vehicles/{vehicleId}/reservations", request, ct);
        return id;
    }

    public Task<IReadOnlyList<ReservationRequestDto>?> GetListingReservationsAsync(
        Guid listingId, string? status = null, CancellationToken ct = default)
    {
        var query = string.IsNullOrWhiteSpace(status) ? string.Empty : $"?status={Uri.EscapeDataString(status)}";
        return SafeGetAsync<IReadOnlyList<ReservationRequestDto>>(
            $"api/v1/listings/{listingId}/reservations{query}", ct);
    }

    public Task<IReadOnlyList<MyReservationDto>?> GetMyReservationsAsync(CancellationToken ct = default)
        => SafeGetAsync<IReadOnlyList<MyReservationDto>>("api/v1/me/reservations", ct);

    public Task<bool> AcceptReservationAsync(Guid reservationId, string? note, CancellationToken ct = default)
        => SafeMutateAsync(HttpMethod.Post, $"api/v1/reservations/{reservationId}/accept",
            new ReservationDecisionRequest(note), ct);

    public Task<bool> RejectReservationAsync(Guid reservationId, string? note, CancellationToken ct = default)
        => SafeMutateAsync(HttpMethod.Post, $"api/v1/reservations/{reservationId}/reject",
            new ReservationDecisionRequest(note), ct);

    public Task<bool> CancelReservationAsync(Guid reservationId, string? note, CancellationToken ct = default)
        => SafeMutateAsync(HttpMethod.Post, $"api/v1/reservations/{reservationId}/cancel",
            new ReservationDecisionRequest(note), ct);

    public Task<IReadOnlyList<MyListingDto>?> GetMyListingsAsync(CancellationToken ct = default)
        => SafeGetAsync<IReadOnlyList<MyListingDto>>("api/v1/me/listings", ct);

    public async Task<ContactResultDto?> ContactProviderAsync(Guid listingId, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.PostAsJsonAsync(
                $"api/v1/listings/{listingId}/contact",
                new { WaiverAccepted = true },
                ct);

            if (response.StatusCode == HttpStatusCode.Unauthorized ||
                response.StatusCode == HttpStatusCode.Forbidden)
            {
                _logger.LogWarning("ContactProvider: not authenticated or forbidden for listing {ListingId}", listingId);
                return null;
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ContactResultDto>(ct);
        }
        catch (HttpRequestException ex) when ((int?)ex.StatusCode >= 500)
        {
            _logger.LogError(ex, "Server error contacting provider for listing {ListingId}", listingId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error contacting provider for listing {ListingId}", listingId);
            throw;
        }
    }

    // ----------------------------------------------------------------
    // Internal helpers
    // ----------------------------------------------------------------

    private async Task<T?> SafeGetAsync<T>(string url, CancellationToken ct) where T : class
    {
        try
        {
            var response = await _http.GetAsync(url, ct);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized ||
                response.StatusCode == HttpStatusCode.Forbidden)
            {
                _logger.LogWarning("Unauthorized access to {Url}", url);
                return null;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>(ct);
        }
        catch (HttpRequestException ex) when ((int?)ex.StatusCode >= 500)
        {
            _logger.LogError(ex, "Server error fetching {Url}", url);
            throw;
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("Request to {Url} was cancelled or timed out", url);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching {Url}", url);
            throw;
        }
    }

    private async Task<(Guid? Id, HttpStatusCode Status)> SafePostForIdAsync<TBody>(
        string url, TBody body, CancellationToken ct)
    {
        try
        {
            var response = await _http.PostAsJsonAsync(url, body, ct);
            if (response.StatusCode is
                HttpStatusCode.Unauthorized
                or HttpStatusCode.Forbidden
                or HttpStatusCode.NotFound
                or HttpStatusCode.Conflict
                or HttpStatusCode.BadRequest)
                return (null, response.StatusCode);

            response.EnsureSuccessStatusCode();
            var payload = await response.Content.ReadFromJsonAsync<IdResponse>(ct);
            return (payload?.Id, response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "POST {Url} failed", url);
            throw;
        }
    }

    private Task<bool> SafeMutateAsync(HttpMethod method, string url, CancellationToken ct)
        => SafeMutateInternalAsync(method, url, content: null, ct);

    private Task<bool> SafeMutateAsync<TBody>(
        HttpMethod method, string url, TBody body, CancellationToken ct) where TBody : notnull
        => SafeMutateInternalAsync(method, url, JsonContent.Create(body), ct);

    private async Task<bool> SafeMutateInternalAsync(
        HttpMethod method, string url, HttpContent? content, CancellationToken ct)
    {
        try
        {
            using var request = new HttpRequestMessage(method, url);
            if (content is not null) request.Content = content;

            var response = await _http.SendAsync(request, ct);

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden or HttpStatusCode.NotFound)
                return false;

            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Method} {Url} failed", method, url);
            throw;
        }
    }

    private record IdResponse(Guid Id);

    private static string BuildQueryString(Dictionary<string, string?> parameters)
    {
        var parts = parameters
            .Where(kv => kv.Value is not null)
            .Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value!)}");
        return string.Join("&", parts);
    }
}
