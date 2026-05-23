using Bulletin.Board.Domain.Interfaces.Services;
using Bulletin.Board.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Infrastructure.Services;

public sealed class UserService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ILogger<UserService> logger) : IUserService
{
    public async Task<Guid> CreateUserAsync(string email, string password, string role,
        string? firstName = null, string? lastName = null, CancellationToken ct = default)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = firstName,
            LastName = lastName
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"User registration failed: {errors}");
        }

        var roleResult = await userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            var errors = string.Join("; ", roleResult.Errors.Select(e => e.Description));
            logger.LogWarning("Failed to assign role {Role} to user {UserId}: {Errors}", role, user.Id, errors);
        }

        return user.Id;
    }

    public async Task<(bool Success, Guid UserId, string Email, IEnumerable<string> Roles)> ValidateCredentialsAsync(
        string email, string password, CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null || !user.IsActive)
            return (false, Guid.Empty, string.Empty, []);

        var result = await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
        if (!result.Succeeded)
            return (false, Guid.Empty, string.Empty, []);

        var roles = await userManager.GetRolesAsync(user);
        return (true, user.Id, user.Email ?? email, roles);
    }

    public async Task<(string Email, IEnumerable<string> Roles, string? FullName)> GetUserInfoAsync(
        Guid userId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return (string.Empty, [], null);
        var roles = await userManager.GetRolesAsync(user);
        var fullName = BuildFullName(user.FirstName, user.LastName);
        return (user.Email ?? string.Empty, roles, fullName);
    }

    private static string? BuildFullName(string? first, string? last)
    {
        var parts = new[] { first, last }.Where(p => !string.IsNullOrWhiteSpace(p));
        var joined = string.Join(' ', parts);
        return string.IsNullOrWhiteSpace(joined) ? null : joined;
    }

    public async Task<IReadOnlyDictionary<Guid, UserNameInfo>> GetNamesAsync(
        IEnumerable<Guid> userIds, CancellationToken ct = default)
    {
        var ids = userIds.ToHashSet();
        if (ids.Count == 0) return new Dictionary<Guid, UserNameInfo>();

        var rows = await userManager.Users
            .Where(u => ids.Contains(u.Id))
            .Select(u => new { u.Id, u.FirstName, u.LastName })
            .ToListAsync(ct);

        return rows.ToDictionary(r => r.Id, r => new UserNameInfo(r.FirstName, r.LastName));
    }
}
