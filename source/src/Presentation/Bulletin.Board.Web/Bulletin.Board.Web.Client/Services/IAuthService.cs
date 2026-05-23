namespace Bulletin.Board.Web.Client.Services;

/// <summary>
/// Contract for authentication operations.
/// The access token lives exclusively in memory (never in localStorage or
/// sessionStorage). The refresh-token is an HttpOnly cookie managed by the browser.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// True when an access token is in memory and has not yet expired.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Authenticates the user. On success the access token is stored in memory
    /// and the server sets the HttpOnly refresh-token cookie.
    /// </summary>
    Task<bool> LoginAsync(string email, string password, bool rememberMe = false, CancellationToken ct = default);

    /// <summary>
    /// Registers a new user. Returns true on success.
    /// </summary>
    Task<bool> RegisterAsync(string email, string password, string role, CancellationToken ct = default);

    /// <summary>
    /// Calls POST /auth/logout (which revokes the refresh-token cookie server-side)
    /// and clears the in-memory access token.
    /// </summary>
    Task LogoutAsync(CancellationToken ct = default);

    /// <summary>
    /// Attempts a silent refresh using the HttpOnly cookie. Returns true if a new
    /// access token was obtained and stored in memory.
    /// </summary>
    Task<bool> TryRefreshAsync(CancellationToken ct = default);

    /// <summary>
    /// Returns the current in-memory access token synchronously (null if not
    /// authenticated or expired). Used by AuthenticatedHttpHandler for the retry path.
    /// </summary>
    string? GetCurrentToken();

    /// <summary>
    /// Returns the current access token, triggering a proactive refresh if it
    /// expires within 60 seconds. Safe to await from HTTP message handlers.
    /// </summary>
    Task<string?> GetTokenAsync(CancellationToken ct = default);

    /// <summary>
    /// Returns the current access token. Alias of GetTokenAsync for clarity.
    /// </summary>
    Task<string?> GetAccessTokenAsync(CancellationToken ct = default);

    /// <summary>
    /// Stores the access token in memory with its TTL. Called externally when the
    /// token is obtained through a path other than LoginAsync (e.g. SSO).
    /// </summary>
    Task SetAccessTokenAsync(string token, int expiresIn);

    /// <summary>
    /// Clears the in-memory access token. Does NOT call the logout endpoint.
    /// Use when the session is known to be dead (e.g. after a failed refresh).
    /// </summary>
    Task ClearAsync();

    /// <summary>
    /// The last API error message, if any. Cleared on each new request.
    /// </summary>
    string? LastError { get; }
}
