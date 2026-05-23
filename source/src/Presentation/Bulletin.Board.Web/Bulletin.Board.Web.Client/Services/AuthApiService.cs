using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Text.Json;
using Bulletin.Board.Web.Client.Models;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace Bulletin.Board.Web.Client.Services;

/// <summary>
/// Authentication service: stores the JWT in localStorage so it survives F5 and
/// forceLoad redirects. Login -> POST /auth/login -> save token -> caller decides
/// the post-login route based on JWT role claim.
/// </summary>
public sealed class AuthApiService : IAuthService
{
    private const string TokenKey   = "bb_token";
    private const string ExpiresKey = "bb_token_exp";

    private readonly HttpClient _http;
    private readonly ILogger<AuthApiService> _logger;
    private readonly IJSRuntime _js;

    private string? _accessToken;
    private DateTime _expiresAtUtc = DateTime.MinValue;

    private string? _lastError;
    public string? LastError => _lastError;

    public bool IsAuthenticated =>
        _accessToken is not null && DateTime.UtcNow < _expiresAtUtc;

    public AuthApiService(
        HttpClient http,
        ILogger<AuthApiService> logger,
        IJSRuntime js)
    {
        _http   = http;
        _logger = logger;
        _js     = js;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // IAuthService
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<bool> LoginAsync(
        string email,
        string password,
        bool rememberMe = false,
        CancellationToken ct = default)
    {
        _lastError = null;
        try
        {
            var response = await _http.PostAsJsonAsync(
                "api/v1/auth/login",
                new { Email = email, Password = password, RememberMe = rememberMe },
                ct);

            if (!response.IsSuccessStatusCode)
            {
                _lastError = await ExtractApiErrorAsync(response, ct);
                _logger.LogWarning("Login failed with status {Status}", response.StatusCode);
                return false;
            }

            var result = await response.Content.ReadFromJsonAsync<LoginResultDto>(ct);
            if (result is null) return false;

            SetToken(result.AccessToken, result.ExpiresIn);
            return true;
        }
        catch (Exception ex)
        {
            _lastError = null;
            _logger.LogError(ex, "Login request failed");
            return false;
        }
    }

    public async Task<bool> RegisterAsync(
        string email,
        string password,
        string role,
        CancellationToken ct = default)
    {
        _lastError = null;
        try
        {
            var response = await _http.PostAsJsonAsync(
                "api/v1/auth/register",
                new { Email = email, Password = password, Role = role },
                ct);

            if (!response.IsSuccessStatusCode)
            {
                _lastError = await ExtractApiErrorAsync(response, ct);
                _logger.LogWarning("Registration failed with status {Status}", response.StatusCode);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _lastError = null;
            _logger.LogError(ex, "Registration request failed");
            return false;
        }
    }

    public async Task LogoutAsync(CancellationToken ct = default)
    {
        try
        {
            // credentials: include is handled by the browser automatically for same-site
            // cookies; in WASM the underlying fetch API sends cookies by default.
            await _http.PostAsync("api/v1/auth/logout", null, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Logout endpoint call failed (continuing with local cleanup)");
        }
        finally
        {
            ClearToken();
        }
    }

    public async Task<bool> TryRefreshAsync(CancellationToken ct = default)
    {
        _lastError = null;
        try
        {
            // The browser sends the HttpOnly refresh-token cookie automatically.
            var response = await _http.PostAsync("api/v1/auth/refresh", null, ct);
            if (!response.IsSuccessStatusCode) return false;

            var result = await response.Content.ReadFromJsonAsync<LoginResultDto>(ct);
            if (result is null) return false;

            SetToken(result.AccessToken, result.ExpiresIn);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token refresh failed");
            return false;
        }
    }

    public string? GetCurrentToken() => IsAuthenticated ? _accessToken : null;

    public async Task<string?> GetTokenAsync(CancellationToken ct = default)
    {
        if (_accessToken is null)
            await LoadTokenFromStorageAsync();

        if (_accessToken is not null
            && _expiresAtUtc != DateTime.MinValue
            && DateTime.UtcNow >= _expiresAtUtc.AddSeconds(-60))
        {
            await TryRefreshAsync(ct);
        }

        return GetCurrentToken();
    }

    public Task<string?> GetAccessTokenAsync(CancellationToken ct = default)
        => GetTokenAsync(ct);

    public Task SetAccessTokenAsync(string token, int expiresIn)
    {
        SetToken(token, expiresIn);
        return Task.CompletedTask;
    }

    public Task ClearAsync()
    {
        ClearToken();
        return Task.CompletedTask;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Internal helpers — kept internal so BulletinAuthStateProvider can call
    // LoadTokenFromStorageAsync() with the same signature as before (no-op now).
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Loads the JWT from localStorage into memory. Called by
    /// BulletinAuthStateProvider.InitializeAsync() at app startup so that an F5
    /// reload restores the session.
    /// </summary>
    internal async Task<string?> LoadTokenFromStorageAsync()
    {
        if (_accessToken is not null && DateTime.UtcNow < _expiresAtUtc)
            return _accessToken;

        try
        {
            var token = await _js.InvokeAsync<string?>("localStorage.getItem", TokenKey);
            if (string.IsNullOrWhiteSpace(token)) return null;

            var expiry = ParseJwtExpiry(token);
            if (!expiry.HasValue || DateTime.UtcNow >= expiry.Value)
            {
                await RemoveFromStorageAsync();
                return null;
            }

            _accessToken  = token;
            _expiresAtUtc = expiry.Value;
            return _accessToken;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not load token from localStorage");
            return null;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Private helpers
    // ─────────────────────────────────────────────────────────────────────────

    private void SetToken(string token, int expiresInSeconds)
    {
        _accessToken  = token;
        _expiresAtUtc = DateTime.UtcNow.AddSeconds(expiresInSeconds);
        var jwtExpiry = ParseJwtExpiry(token);
        if (jwtExpiry.HasValue)
            _expiresAtUtc = jwtExpiry.Value;

        _ = PersistToStorageAsync(token);
    }

    private void ClearToken()
    {
        _accessToken  = null;
        _expiresAtUtc = DateTime.MinValue;
        _ = RemoveFromStorageAsync();
    }

    private async Task PersistToStorageAsync(string token)
    {
        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", TokenKey, token);
            await _js.InvokeVoidAsync("localStorage.setItem", ExpiresKey,
                _expiresAtUtc.ToString("O"));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not persist token to localStorage");
        }
    }

    private async Task RemoveFromStorageAsync()
    {
        try
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
            await _js.InvokeVoidAsync("localStorage.removeItem", ExpiresKey);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not remove token from localStorage");
        }
    }

    private async Task<string?> GetTokenWithProactiveRefreshAsync(CancellationToken ct)
    {
        _logger.LogInformation(
            "Access token expires in less than 60 s — refreshing proactively");
        var ok = await TryRefreshAsync(ct);
        return ok ? _accessToken : null;
    }

    private static DateTime? ParseJwtExpiry(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(token)) return null;
            var jwt = handler.ReadJwtToken(token);
            return jwt.ValidTo == DateTime.MinValue ? null : jwt.ValidTo;
        }
        catch
        {
            return null;
        }
    }

    private static async Task<string?> ExtractApiErrorAsync(
        HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            if (string.IsNullOrWhiteSpace(body)) return null;

            using var doc  = JsonDocument.Parse(body);
            var       root = doc.RootElement;

            if (root.TryGetProperty("errors", out var errors))
            {
                foreach (var field in errors.EnumerateObject())
                {
                    if (field.Value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var msg in field.Value.EnumerateArray())
                        {
                            var text = msg.GetString();
                            if (!string.IsNullOrWhiteSpace(text)) return text;
                        }
                    }
                }
            }

            if (root.TryGetProperty("detail", out var detail))
            {
                var text = detail.GetString();
                if (!string.IsNullOrWhiteSpace(text)) return text;
            }

            if (root.TryGetProperty("title", out var title))
            {
                var text = title.GetString();
                if (!string.IsNullOrWhiteSpace(text)) return text;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}
