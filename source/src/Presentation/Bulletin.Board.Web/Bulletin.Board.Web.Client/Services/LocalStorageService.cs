using Microsoft.JSInterop;

namespace Bulletin.Board.Web.Client.Services;

/// <summary>
/// Thin wrapper over browser localStorage via JS interop.
/// All methods are safe to call during Blazor WASM startup because
/// they use IJSRuntime (async); they must NOT be called during SSR
/// pre-render (IJSRuntime throws in that context).
/// </summary>
public sealed class LocalStorageService
{
    private readonly IJSRuntime _js;

    public LocalStorageService(IJSRuntime js) => _js = js;

    public async Task SetItemAsync(string key, string value)
    {
        await _js.InvokeVoidAsync("localStorage.setItem", key, value);
    }

    public async Task<string?> GetItemAsync(string key)
    {
        return await _js.InvokeAsync<string?>("localStorage.getItem", key);
    }

    public async Task RemoveItemAsync(string key)
    {
        await _js.InvokeVoidAsync("localStorage.removeItem", key);
    }
}
