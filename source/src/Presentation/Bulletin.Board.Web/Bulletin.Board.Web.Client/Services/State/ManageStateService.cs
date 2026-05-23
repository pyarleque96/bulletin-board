using Bulletin.Board.Web.Client.Models;
using Bulletin.Board.Web.Client.Services.Common;
using Bulletin.Board.Web.Client.Services.Http;

namespace Bulletin.Board.Web.Client.Services.State;

/// <summary>
/// Scoped state container for the /manage panel.
/// Caches the currently selected listing so detail sub-pages
/// (Overview, Vehicles, Reservations) do not each reload independently.
///
/// Lifecycle: scoped per Blazor WASM tab session (one instance per circuit).
/// Call <see cref="LoadListingAsync"/> on initial mount; call <see cref="Invalidate"/>
/// after any mutation that changes listing status, then re-fetch with <see cref="ReloadAsync"/>.
/// </summary>
public sealed class ManageStateService : IDisposable
{
    private readonly IListingService _listingService;

    // ── State ──────────────────────────────────────────────────────────────────

    /// <summary>The Id of the currently open listing in the manage panel.</summary>
    public Guid? CurrentListingId { get; private set; }

    /// <summary>
    /// The cached listing detail. Null until <see cref="LoadListingAsync"/> completes
    /// or when the cache is invalidated.
    /// </summary>
    public ListingDetailDto? CurrentListing { get; private set; }

    /// <summary>
    /// Last load result; null when nothing has been loaded yet.
    /// Exposed so UI can distinguish Loading/Success/Error without re-fetching.
    /// </summary>
    public ApiResult<ListingDetailDto>? LastResult { get; private set; }

    // ── Notifications ──────────────────────────────────────────────────────────

    /// <summary>Raised whenever state changes — components should call StateHasChanged().</summary>
    public event Action? OnStateChanged;

    // ── Constructor ────────────────────────────────────────────────────────────

    public ManageStateService(IListingService listingService)
    {
        _listingService = listingService;
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Loads (or reuses cached) listing detail for <paramref name="id"/>.
    /// If the same Id is already loaded, this is a no-op (use <see cref="ReloadAsync"/> to force).
    /// </summary>
    public async Task LoadListingAsync(Guid id, CancellationToken ct = default)
    {
        if (CurrentListingId == id && CurrentListing is not null)
            return; // Cache hit — no round-trip needed.

        CurrentListingId = id;
        CurrentListing   = null;
        LastResult       = ApiResult<ListingDetailDto>.IsLoading();
        NotifyStateChanged();

        await FetchAsync(id, ct);
    }

    /// <summary>
    /// Invalidates the cache without triggering a fetch.
    /// Call this before a mutation so the next <see cref="LoadListingAsync"/> forces a refresh.
    /// </summary>
    public void Invalidate()
    {
        CurrentListing = null;
        LastResult     = null;
        NotifyStateChanged();
    }

    /// <summary>
    /// Forces a fresh fetch for the current listing id.
    /// No-op if <see cref="CurrentListingId"/> is null.
    /// </summary>
    public async Task ReloadAsync(CancellationToken ct = default)
    {
        if (CurrentListingId is not Guid id) return;

        Invalidate();
        await FetchAsync(id, ct);
    }

    // ── Internal ───────────────────────────────────────────────────────────────

    private async Task FetchAsync(Guid id, CancellationToken ct)
    {
        var result = await _listingService.GetListingByIdAsync(id, ct);
        LastResult = result;

        if (result is ApiResult<ListingDetailDto>.Success s)
            CurrentListing = s.Data;

        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnStateChanged?.Invoke();

    public void Dispose()
    {
        // No unmanaged resources; clear delegates to allow GC.
        OnStateChanged = null;
    }
}
