using Bulletin.Board.Web.Client.Models;

namespace Bulletin.Board.Web.Client.Services;

/// <summary>
/// Contract for category discovery (public) and management (Admin only).
/// </summary>
public interface ICategoriesService
{
    /// <summary>
    /// Returns all active categories ordered by listing count descending.
    /// Returns empty array on error (non-critical; UI degrades gracefully).
    /// </summary>
    Task<CategoryDto[]> GetAllAsync(bool includeCount = false, CancellationToken ct = default);

    // ── Admin-only operations ────────────────────────────────────────────
    // All methods below require a JWT with role "Admin" attached to the
    // outgoing request (auth cookie attached by the browser).

    /// <summary>
    /// Returns ALL categories including inactive ones — for the admin panel.
    /// Throws <see cref="HttpRequestException"/> on failure so callers can
    /// surface the error explicitly (unlike the public GetAllAsync).
    /// </summary>
    Task<CategoryAdminDto[]> GetAllAdminAsync(CancellationToken ct = default);

    /// <summary>Creates a new category. Returns the created DTO.</summary>
    Task<CategoryAdminDto> CreateAsync(CategoryCreateModel model, CancellationToken ct = default);

    /// <summary>Updates an existing category. Server responds 204 No Content on success.</summary>
    Task UpdateAsync(Guid id, CategoryUpdateModel model, CancellationToken ct = default);

    /// <summary>Soft-deletes (deactivates) a category by ID.</summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
