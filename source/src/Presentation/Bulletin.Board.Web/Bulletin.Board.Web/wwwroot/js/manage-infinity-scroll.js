// manage-infinity-scroll.js — IntersectionObserver module para /manage.
//
// Uso desde Blazor:
//   const module = await JS.InvokeAsync<IJSObjectReference>(
//       "import", "./js/manage-infinity-scroll.js");
//   const handle  = await module.invokeMethodAsync(
//       "init", sentinelElement, dotNetRef);
//   // ...
//   await handle.dispose();
//
// El componente Blazor expone [JSInvokable] LoadMoreAsync() y este módulo lo
// llama cada vez que el sentinel entra al viewport (con rootMargin: 200px).
// Soporta múltiples instancias (devolvemos un "handle" por cada init).

const handles = new WeakMap();

export function init(sentinel, dotNetRef) {
    if (!sentinel || !dotNetRef) {
        return { dispose: () => { } };
    }

    // Si no hay IntersectionObserver, devolvemos un handle inerte — el
    // componente Blazor mostrará el botón "Load more" como fallback.
    if (typeof IntersectionObserver === "undefined") {
        return {
            dispose: () => { try { dotNetRef.dispose && dotNetRef.dispose(); } catch { } }
        };
    }

    let inFlight = false;

    const observer = new IntersectionObserver((entries) => {
        for (const entry of entries) {
            if (!entry.isIntersecting || inFlight) continue;

            inFlight = true;
            dotNetRef.invokeMethodAsync("LoadMoreAsync")
                .catch(err => console.error("[manage-infinity-scroll] LoadMoreAsync failed", err))
                .finally(() => { inFlight = false; });
        }
    }, { rootMargin: "200px" });

    observer.observe(sentinel);
    handles.set(sentinel, observer);

    return {
        dispose: () => {
            try { observer.disconnect(); } catch { }
            try { dotNetRef.dispose && dotNetRef.dispose(); } catch { }
            handles.delete(sentinel);
        }
    };
}
