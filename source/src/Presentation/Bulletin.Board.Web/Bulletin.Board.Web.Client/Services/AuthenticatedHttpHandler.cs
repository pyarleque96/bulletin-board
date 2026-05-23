using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components;

namespace Bulletin.Board.Web.Client.Services;

/// <summary>
/// DelegatingHandler that injects the JWT Bearer token (in-memory only) into
/// every outbound request. On 401, performs a single silent refresh using the
/// HttpOnly cookie and retries the request once, so navigation never breaks
/// mid-session.
///
/// Concurrency safety: a SemaphoreSlim(1,1) ensures only one /auth/refresh
/// call is in flight at any time. Parallel requests that receive 401 wait on
/// the semaphore and re-use the new token instead of each triggering their
/// own refresh (refresh-storm prevention).
/// </summary>
public sealed class AuthenticatedHttpHandler : DelegatingHandler
{
    // Static semaphore: shared across all instances in the same WASM circuit.
    private static readonly SemaphoreSlim _refreshLock = new(1, 1);
    private static long _lastSuccessfulRefreshTicks;

    private readonly IAuthService       _authService;
    private readonly NavigationManager  _navigation;

    public AuthenticatedHttpHandler(
        IAuthService      authService,
        NavigationManager navigation)
    {
        _authService = authService;
        _navigation  = navigation;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken  cancellationToken)
    {
        var token = await _authService.GetTokenAsync(cancellationToken);
        AttachToken(request, token);

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.Unauthorized) return response;
        // Never retry auth endpoints to avoid infinite loops.
        if (IsAuthEndpoint(request.RequestUri)) return response;
        // If we had no token to begin with, there's nothing to refresh.
        if (string.IsNullOrWhiteSpace(token)) return response;

        var refreshed = await TryRefreshOnceAsync(cancellationToken);

        if (!refreshed)
        {
            // Refresh token is expired or session was compromised.
            await _authService.ClearAsync();
            _navigation.NavigateTo("/login?reason=expired", forceLoad: false);
            return response;
        }

        var newToken = _authService.GetCurrentToken();
        if (string.IsNullOrWhiteSpace(newToken)) return response;

        response.Dispose();

        var retry = await CloneAsync(request, cancellationToken);
        AttachToken(retry, newToken);
        return await base.SendAsync(retry, cancellationToken);
    }

    // ─────────────────────────────────────────────────────────────────────────

    private async Task<bool> TryRefreshOnceAsync(CancellationToken ct)
    {
        // Snapshot the marker before queuing so we can detect that another
        // concurrent request already refreshed while we were waiting.
        var snapshotBefore = Interlocked.Read(ref _lastSuccessfulRefreshTicks);

        await _refreshLock.WaitAsync(ct);
        try
        {
            var snapshotAfter = Interlocked.Read(ref _lastSuccessfulRefreshTicks);
            if (snapshotAfter != snapshotBefore)
            {
                // Another request refreshed while we waited — reuse that token.
                return !string.IsNullOrWhiteSpace(_authService.GetCurrentToken());
            }

            var ok = await _authService.TryRefreshAsync(ct);
            if (ok) Interlocked.Exchange(ref _lastSuccessfulRefreshTicks, DateTime.UtcNow.Ticks);
            return ok;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private static void AttachToken(HttpRequestMessage request, string? token)
    {
        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private static bool IsAuthEndpoint(Uri? uri)
    {
        if (uri is null) return false;
        var path = uri.AbsolutePath;
        return path.Contains("/auth/refresh",  StringComparison.OrdinalIgnoreCase)
            || path.Contains("/auth/login",    StringComparison.OrdinalIgnoreCase)
            || path.Contains("/auth/register", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<HttpRequestMessage> CloneAsync(
        HttpRequestMessage request,
        CancellationToken  ct)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version       = request.Version,
            VersionPolicy = request.VersionPolicy,
        };

        foreach (var header in request.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

        if (request.Content is not null)
        {
            var bytes      = await request.Content.ReadAsByteArrayAsync(ct);
            var newContent = new ByteArrayContent(bytes);
            foreach (var h in request.Content.Headers)
                newContent.Headers.TryAddWithoutValidation(h.Key, h.Value);
            clone.Content = newContent;
        }

        return clone;
    }
}
