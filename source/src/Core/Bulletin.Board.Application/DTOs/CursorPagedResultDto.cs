namespace Bulletin.Board.Application.DTOs;

/// <summary>
/// Resultado paginado por cursor opaco. Reemplaza a <see cref="PagedResult{T}"/> en
/// endpoints con infinity scroll (manage v3). El cursor es base64 e incluye el orden
/// canónico completo del listing — ver <c>ManageListingsCursor</c>.
/// </summary>
public record CursorPagedResultDto<T>(
    IReadOnlyList<T> Items,
    string? NextCursor,
    bool HasMore,
    int? TotalItems);
