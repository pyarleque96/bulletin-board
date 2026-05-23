namespace Bulletin.Board.Web.Client.Services.Common;

/// <summary>
/// Client-side mirror of the backend <c>PagedResult&lt;T&gt;</c> contract.
/// Fields are named to match the JSON the server returns after B0 paginaton is applied.
/// Exposes <see cref="HasNext"/> and <see cref="HasPrevious"/> explicitly (the backend
/// computes them; the client does NOT recompute from TotalPages to avoid skew).
/// </summary>
public record PagedResultDto<T>(
    IReadOnlyList<T> Items,
    int              Page,
    int              PageSize,
    int              TotalItems,
    int              TotalPages,
    bool             HasNext,
    bool             HasPrevious,
    bool             HasAnyUnfiltered = true);
