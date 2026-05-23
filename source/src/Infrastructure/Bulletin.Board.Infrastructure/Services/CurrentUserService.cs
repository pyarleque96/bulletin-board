using System.Security.Claims;
using Bulletin.Board.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Http;

namespace Bulletin.Board.Infrastructure.Services;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var value = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public string? Email => User?.FindFirstValue(ClaimTypes.Email);
    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
    public bool IsInRole(string role) => User?.IsInRole(role) ?? false;
}
