namespace Bulletin.Board.Application.DTOs;

/// <summary>
/// Resultado paginado estándar de la plataforma. Todos los endpoints de colección deben
/// devolver este tipo — nunca <see cref="IReadOnlyList{T}"/> sin paginar.
/// </summary>
public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalItems,
    bool HasAnyUnfiltered = true)
{
    public int TotalPages  => PageSize > 0 ? (int)Math.Ceiling((double)TotalItems / PageSize) : 0;
    public bool HasNext    => Page < TotalPages;
    public bool HasPrevious => Page > 1;
}

/// <summary>
/// Parámetros de paginación entrantes. El hard-cap de 100 ítems por página se aplica
/// en servidor — el cliente no puede forzar colecciones completas.
/// </summary>
public record PagedRequest(int Page = 1, int PageSize = 20)
{
    public int Skip => (Math.Max(1, Page) - 1) * Take;
    public int Take => Math.Clamp(PageSize <= 0 ? 20 : PageSize, 1, 100);
}
