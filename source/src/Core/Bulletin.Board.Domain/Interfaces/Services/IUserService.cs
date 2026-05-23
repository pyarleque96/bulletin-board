namespace Bulletin.Board.Domain.Interfaces.Services;

public interface IUserService
{
    Task<Guid> CreateUserAsync(string email, string password, string role,
        string? firstName = null, string? lastName = null, CancellationToken ct = default);

    Task<(bool Success, Guid UserId, string Email, IEnumerable<string> Roles)> ValidateCredentialsAsync(
        string email, string password, CancellationToken ct = default);

    /// <summary>
    /// Devuelve email, roles y nombre completo del usuario.
    /// El nombre completo (FirstName + LastName) se incluye en el claim "name" del JWT.
    /// </summary>
    Task<(string Email, IEnumerable<string> Roles, string? FullName)> GetUserInfoAsync(
        Guid userId, CancellationToken ct = default);

    Task<IReadOnlyDictionary<Guid, UserNameInfo>> GetNamesAsync(
        IEnumerable<Guid> userIds, CancellationToken ct = default);
}

public readonly record struct UserNameInfo(string? FirstName, string? LastName);
