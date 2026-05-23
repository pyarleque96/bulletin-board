using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Web.Client.Services;

/// <summary>
/// Custom AuthenticationStateProvider for Bulletin Dells.
/// Reads the access token from localStorage via AuthApiService,
/// decodes JWT claims, and exposes NotifyAuthenticationStateChanged.
/// </summary>
public sealed class BulletinAuthStateProvider : AuthenticationStateProvider
{
    private readonly AuthApiService _authService;
    private readonly ILogger<BulletinAuthStateProvider> _logger;

    private static readonly AuthenticationState _anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    private readonly TaskCompletionSource<AuthenticationState> _initTcs =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private volatile AuthenticationState _currentState = _anonymous;
    private int _initialized;

    public BulletinAuthStateProvider(
        AuthApiService authService,
        ILogger<BulletinAuthStateProvider> logger)
    {
        _authService = authService;
        _logger      = logger;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (!_initTcs.Task.IsCompleted)
            return _initTcs.Task;

        return Task.FromResult(_currentState);
    }

    public async Task InitializeAsync()
    {
        if (Interlocked.Exchange(ref _initialized, 1) != 0) return;
        try
        {
            var token = await _authService.LoadTokenFromStorageAsync();
            var state = BuildState(token);
            _currentState = state;
            _initTcs.TrySetResult(state);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not determine authentication state");
            _currentState = _anonymous;
            _initTcs.TrySetResult(_anonymous);
        }
    }

    public void NotifyUserLoggedIn(string token)
    {
        var state = BuildState(token);
        _currentState = state;
        _initTcs.TrySetResult(state);
        NotifyAuthenticationStateChanged(Task.FromResult(state));
    }

    public Task WhenInitialized => _initTcs.Task;

    public void NotifyUserLoggedOut()
    {
        _currentState = _anonymous;
        NotifyAuthenticationStateChanged(Task.FromResult(_anonymous));
    }

    private AuthenticationState BuildState(string? token)
    {
        if (string.IsNullOrWhiteSpace(token)) return _anonymous;
        var principal = ParseToken(token);
        return principal is null ? _anonymous : new AuthenticationState(principal);
    }

    private ClaimsPrincipal? ParseToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(token))
                return null;

            var jwt    = handler.ReadJwtToken(token);
            var claims = jwt.Claims.ToList();

            var roleClaim = claims.FirstOrDefault(c =>
                c.Type == "role" || c.Type == ClaimTypes.Role ||
                c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");

            if (roleClaim is not null && roleClaim.Type != ClaimTypes.Role)
            {
                claims.Add(new Claim(ClaimTypes.Role, roleClaim.Value));
            }

            var identity  = new ClaimsIdentity(claims, "jwt");
            var principal = new ClaimsPrincipal(identity);
            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse JWT token");
            return null;
        }
    }
}
