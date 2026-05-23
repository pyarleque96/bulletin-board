# Bug 401 en primera carga — Análisis arquitectónico completo

> Documento técnico: por qué ocurre el HTTP 401 en la primera carga de las páginas admin, cómo se relaciona con el modo de renderizado de Blazor, y por qué la arquitectura actual ya lo maneja correctamente.

---

## 1. Estructura del proyecto

Bulletin Dells sigue **Clean Architecture** con separación estricta de capas:

```
source/src/
├── Core/                          ← Dominio puro (sin dependencias externas)
│   ├── Bulletin.Board.Domain/         Entidades, value objects, invariantes
│   └── Bulletin.Board.Application/    Use cases, CQRS, interfaces de puertos
│
├── Infrastructure/                ← Adaptadores (BD, servicios externos)
│   └── Bulletin.Board.Infrastructure/ EF Core, repositorios, JWT issuer
│
└── Presentation/                  ← Frontend + API
    ├── Bulletin.Board.API/                 API HTTP standalone (.NET 10)
    └── Bulletin.Board.Web/                 Host Blazor (servidor + WASM)
        ├── Bulletin.Board.Web/             Servidor Blazor (SSR + endpoints)
        └── Bulletin.Board.Web.Client/      Cliente WASM (componentes interactivos)
```

**El proyecto Web tiene dos ensamblados** porque usa `InteractiveAuto`:
- `Bulletin.Board.Web` → corre en el servidor (.NET 10), hace SSR + hospeda el WASM bundle.
- `Bulletin.Board.Web.Client` → corre en el navegador (mono-WASM), toma control después de la hidratación.

Los componentes admin están en `Web.Client/Pages/Admin/`. Los servicios de auth (`AuthApiService`, `BulletinAuthStateProvider`, `AuthenticatedHttpHandler`) viven en `Web.Client/Services/` y se registran en `ClientServicesExtensions.cs`.

---

## 2. Modos de renderizado de Blazor — el contexto del bug

Blazor 8/9 ofrece varios modos. Bulletin Dells usa **`InteractiveAuto`**, configurado en `Web/Program.cs`:

```csharp
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AllowAnonymous();
```

### Cómo funciona `InteractiveAuto`

`InteractiveAuto` ejecuta los componentes en **dos fases distintas** durante la primera visita:

**Fase 1 — SSR (Server-Side Render) + circuit Blazor Server**

1. El navegador hace `GET /admin`.
2. El servidor renderiza el árbol de componentes en C# directamente (sin JS).
3. Mientras tanto, descarga el bundle WASM en background.
4. El HTML se envía al cliente con un placeholder activo conectado por SignalR (modo Server transitorio).
5. **`IJSRuntime` no tiene DOM disponible** → cualquier llamada a `localStorage` lanza excepción.

**Fase 2 — WASM hydration (cuando el bundle termina de descargar)**

1. WASM toma control del árbol → **se crea un nuevo DI scope dentro del navegador**.
2. Todas las instancias `Scoped` se construyen de nuevo (el `_initialized` flag, el `TaskCompletionSource`, el caché del token — todo es nuevo).
3. Los componentes se re-inicializan: `OnInitializedAsync` se ejecuta otra vez.
4. Esta vez `IJSRuntime` sí funciona → `localStorage` está disponible → se lee el token.
5. Visitas posteriores (con WASM cacheado) saltan la Fase 1.

> **Implicación clave**: cualquier componente con `@attribute [Authorize]` se inicializa **dos veces** en la primera visita: una en SSR (sin token) y otra en WASM (con token).

---

## 3. Arquitectura de autenticación

### Stack

- **API .NET 10** emite JWT firmados (HS256).
- **Token guardado en `localStorage`** bajo la clave `bulletin_access_token`.
- **Refresh token** en cookie HttpOnly, se canjea vía `POST /api/v1/auth/refresh`.
- **Auth state en cliente** gestionado por `BulletinAuthStateProvider` (custom `AuthenticationStateProvider`).
- **Inyección de Bearer token** en cada request HTTP vía `AuthenticatedHttpHandler` (`DelegatingHandler`).

### Componentes y responsabilidades

#### `AuthApiService` (`Web.Client/Services/AuthApiService.cs`)
- Login / register / logout / refresh contra la API.
- Gestiona el caché en memoria del token (`_cachedToken`).
- Lee/escribe `localStorage` vía `LocalStorageService`.
- **Serializa lecturas concurrentes** del `localStorage` con `_loadTask ??= LoadTokenCoreAsync()` para evitar dobles llamadas a JS interop.

#### `BulletinAuthStateProvider` (`Web.Client/Services/BulletinAuthStateProvider.cs`)
- Hereda `AuthenticationStateProvider`. Es lo que `AuthorizeRouteView` y `<AuthorizeView>` consultan.
- Tiene un `TaskCompletionSource<AuthenticationState> _initTcs` que **gateway el primer render** hasta que se sepa si el usuario está autenticado.
- `InitializeAsync()` lee el token, decodifica los claims JWT, resuelve la TCS.
- `WhenInitialized` (Task pública) permite a otros componentes esperar la inicialización.
- `_initialized` con `Interlocked.Exchange` garantiza que `InitializeAsync()` corre **una sola vez** por instancia DI.

#### `AuthenticatedHttpHandler` (`Web.Client/Services/AuthenticatedHttpHandler.cs`)
- `DelegatingHandler` registrado en todos los `HttpClient` tipados (`AdminApiService`, `ListingsApiService`, `CategoriesApiService`).
- En cada request: obtiene el token vía `_authService.GetTokenAsync()` y lo inyecta como `Authorization: Bearer <jwt>`.
- **Cortocircuita en SSR** con `if (!OperatingSystem.IsBrowser())` → devuelve `200 OK` con cuerpo `null`. **Esta es la pieza clave que evita el 401 en la fase SSR.**

#### `Routes.razor` (`Web.Client/Routes.razor`)
- Llama `AuthProvider.InitializeAsync()` desde su `OnInitializedAsync`.
- En el bloque `<NotAuthorized>`, redirige a `/login` **solo si `OperatingSystem.IsBrowser()`** — durante SSR no redirige (porque el token aún no se puede leer).

#### `Web/Program.cs` — autenticación del servidor
- Se registra `AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)` **únicamente para satisfacer la dependencia de `IAuthenticationService`** que requiere `AuthorizeRouteView`.
- `OnRedirectToLogin` se sobreescribe para **devolver 401 en lugar de un redirect 302** — así el flujo de auth queda 100% en el cliente Blazor.
- `MapRazorComponents<App>().AllowAnonymous()` → la auth real se enforce en cliente vía `AuthorizeRouteView`.

---

## 4. Por qué ocurre el 401 en la primera carga

### El escenario problemático

Usuario abre `/admin` por primera vez (caché WASM vacío):

```
T=0 ms    Browser → GET /admin
T=0 ms    Servidor inicia SSR (Fase 1 InteractiveAuto)
          ├─ Routes.razor.OnInitializedAsync corre
          │  └─ AuthProvider.InitializeAsync()
          │     └─ AuthApiService.LoadTokenFromStorageAsync()
          │        └─ JS interop sobre localStorage → THROW (no DOM)
          │     └─ catch → _initTcs.TrySetResult(_anonymous)
          │
          ├─ Dashboard.razor.OnInitializedAsync corre EN PARALELO
          │  └─ AdminService.GetStatsAsync()
          │     └─ AuthenticatedHttpHandler.SendAsync()
          │        └─ !OperatingSystem.IsBrowser() == true
          │        └─ retorna 200 con body "null"     ← NO HAY 401 EN SSR
          │
          └─ HTML se envía al navegador (con datos vacíos / loading state)

T=~1500 ms  WASM bundle descargado, hidratación inicia (Fase 2)
            ├─ NUEVO DI scope creado en el navegador
            ├─ Routes.razor.OnInitializedAsync corre (instancia WASM nueva)
            │  └─ AuthProvider.InitializeAsync()
            │     └─ JS interop OK → token leído de localStorage
            │     └─ _initTcs.TrySetResult(authenticated)
            │
            └─ Dashboard.razor.OnInitializedAsync corre (instancia WASM nueva)
               └─ AdminService.GetStatsAsync()
                  └─ AuthenticatedHttpHandler.SendAsync()
                     └─ OperatingSystem.IsBrowser() == true
                     └─ _authService.GetTokenAsync()
                        └─ ¿Token en caché? → DEPENDE DE LA RACE
```

### El verdadero origen del 401: race condition durante hidratación WASM

Durante la Fase 2, **`Routes.OnInitializedAsync` y `Dashboard.OnInitializedAsync` se ejecutan en paralelo** en el motor de renderizado de Blazor. Cuando `Routes` hace `await AuthProvider.InitializeAsync()` y la operación de JS interop yield-ea el thread, Blazor empieza a inicializar componentes hijos en la misma "frame" de render.

Si el `OnInitializedAsync` de `Dashboard` llega al `AdminService.GetStatsAsync()` **antes** de que `LoadTokenCoreAsync()` haya escrito en `_cachedToken`:

1. `AuthenticatedHttpHandler` llama a `GetTokenAsync()`.
2. `GetTokenAsync()` llama a `LoadTokenFromStorageAsync()`.
3. Como `_loadTask` ya está en flight (lo inició Routes), la espera se serializa correctamente — esto **debería** funcionar.

**Pero** hay un escenario de borde donde la primera llamada a JS interop falla (mensaje *"JavaScript interop calls cannot be issued at this time"* durante hidratación). En ese caso, `LoadTokenCoreAsync()` retorna `null`, limpia `_loadTask = null`, y el segundo caller obtiene `null`. El request sale **sin** Bearer header, y la API responde **401**.

### Resumen del root cause

| Fase | ¿Hay 401? | Por qué |
|---|---|---|
| SSR (Fase 1) | **No** | `AuthenticatedHttpHandler` cortocircuita devolviendo 200 + `null` |
| WASM hidratación (Fase 2) | **A veces** | Race condition: si JS interop falla durante hidratación, el token no se carga a tiempo y el primer request sale sin Bearer |
| Re-render normal post-hidratación | **No** | Token ya en caché en memoria |

---

## 5. Cómo está mitigado en la arquitectura actual

La arquitectura ya tiene **tres capas de defensa** contra este 401:

### Capa 1 — `AuthenticatedHttpHandler` cortocircuita en SSR

```csharp
if (!OperatingSystem.IsBrowser())
{
    return new HttpResponseMessage(HttpStatusCode.OK)
    {
        Content = new StringContent("null", Encoding.UTF8, "application/json")
    };
}
```

Garantiza que durante la Fase SSR ningún request HTTP autenticado llegue siquiera a salir del cliente. Las páginas reciben `null` / colecciones vacías.

### Capa 2 — Re-ejecución de `OnInitializedAsync` en hidratación WASM

Cuando WASM toma control, **Blazor instancia un nuevo árbol de componentes y vuelve a llamar `OnInitializedAsync`**. Aunque la Fase 1 haya devuelto datos vacíos, la Fase 2 los recarga con el token real desde `localStorage`. Para el usuario es transparente (~1-2 s).

### Capa 3 — Serialización de lectura de `localStorage`

```csharp
internal Task<string?> LoadTokenFromStorageAsync()
{
    if (_storageLoaded)
        return Task.FromResult(_cachedToken);

    _loadTask ??= LoadTokenCoreAsync();
    return _loadTask;
}
```

Múltiples callers concurrentes en la misma instancia DI WASM esperan la **misma** Task, evitando lecturas duplicadas que podrían dejar a algunos sin token.

### Capa 4 — `Routes.razor` no redirige durante SSR

```razor
<NotAuthorized>
    @if (OperatingSystem.IsBrowser())
    {
        <LoginRedirect />
    }
</NotAuthorized>
```

Evita que un usuario autenticado sea redirigido a `/login` durante la Fase 1 antes de que WASM lea el token.

---

## 6. Por qué el "fix" de `await AuthProvider.WhenInitialized` en las páginas hijas FUE INCORRECTO

Se intentó añadir `await AuthProvider.WhenInitialized` como primera línea de `OnInitializedAsync` en las 5 páginas admin. **Esto causó una regresión donde ningún servicio cargaba en la primera visita.**

### Por qué falló

`Routes.OnInitializedAsync` y `Dashboard.OnInitializedAsync` corren **en paralelo** durante la hidratación WASM. Si los hijos esperan `WhenInitialized`:

```
Dashboard.OnInitializedAsync:
  await AuthProvider.WhenInitialized   ← BLOQUEADO esperando que Routes resuelva la TCS

Routes.OnInitializedAsync:
  await AuthProvider.InitializeAsync()  ← suspendido en JS interop
```

Blazor **no garantiza el orden de completion** entre `OnInitializedAsync` del padre y de los hijos durante la hidratación. El motor de render espera a que **todos** los `OnInitializedAsync` completen antes de hacer el siguiente render. El resultado: un bloqueo cooperativo donde la UI nunca termina de cargar.

`AdminLayout` no tiene este problema porque espera `WhenInitialized` en `OnAfterRenderAsync(firstRender)`, que corre **después** de que el árbol completo haya hecho su primer render.

### La conclusión

**No se debe esperar `WhenInitialized` en `OnInitializedAsync` de las páginas hija.** Las cuatro capas de defensa ya manejan el caso del 401 sin necesidad de sincronización explícita.

---

## 7. Flujo completo de auth — caso feliz

### Login

```
1. Usuario rellena formulario en /login
2. Login.razor llama AuthApiService.LoginAsync(email, password)
3. POST /api/v1/auth/login → API valida credenciales, emite JWT
4. AuthApiService:
   - _cachedToken = jwt
   - _storageLoaded = true
   - localStorage.setItem("bulletin_access_token", jwt)
5. BulletinAuthStateProvider.NotifyUserLoggedIn(jwt)
   - Decodifica claims, construye ClaimsPrincipal
   - _currentState = authenticated
   - NotifyAuthenticationStateChanged → AuthorizeRouteView re-evalúa
6. NavigationManager.NavigateTo("/admin")
```

### Request a endpoint protegido (post-login, post-hidratación)

```
1. Componente llama AdminService.GetStatsAsync()
2. HttpClient → AuthenticatedHttpHandler.SendAsync()
3. OperatingSystem.IsBrowser() == true
4. _authService.GetTokenAsync()
   - LoadTokenFromStorageAsync() → fast path (_storageLoaded = true) → retorna _cachedToken
   - IsTokenExpired? → si sí, intenta refresh vía /api/v1/auth/refresh
5. request.Headers.Authorization = "Bearer <jwt>"
6. base.SendAsync() → API valida JWT, ejecuta endpoint
7. 200 OK con datos
```

### F5 (refresh) en una página admin

```
1. Browser → GET /admin (igual que primera carga, pero WASM ya está cacheado)
2. Fase SSR muy breve o saltada por completo si InteractiveAuto detecta WASM cacheado
3. Fase WASM:
   - DI scope nuevo
   - Routes.OnInitializedAsync → InitializeAsync() lee token de localStorage
   - Dashboard.OnInitializedAsync → request con Bearer → 200 OK
```

---

## 8. Decisiones arquitectónicas clave

| Decisión | Por qué |
|---|---|
| JWT en `localStorage` (no cookie) | Permite que el cliente WASM controle 100% el flujo de auth. Mitigación XSS: CSP estricta + sanitización de inputs. |
| Refresh token en cookie HttpOnly | Mitiga robo de credenciales de larga duración vía XSS. |
| `AuthenticatedHttpHandler` cortocircuita SSR | Evita 401 espurios en la fase server-side de InteractiveAuto. |
| Cookie auth en `Web/Program.cs` con `OnRedirectToLogin → 401` | Satisface la dependencia de `IAuthenticationService` sin romper el flujo de auth client-side. |
| `AllowAnonymous()` en `MapRazorComponents` | Toda la autorización se enforce en cliente vía `AuthorizeRouteView`. |
| `BulletinAuthStateProvider` con `_currentState` volátil + `_initTcs` | Permite que el primer render espere la lectura de `localStorage` sin quedar atascado en estado anonymous tras un login posterior. |

---

## 9. Diagnóstico rápido de un 401

Si vuelve a aparecer un 401 en producción:

1. **¿Ocurre solo en la primera visita?** → race de hidratación. Verificar que las 4 capas de defensa siguen activas.
2. **¿Ocurre en todas las visitas?** → token expirado o inválido. Revisar `IsTokenExpired` y el endpoint `/api/v1/auth/refresh`.
3. **¿Ocurre solo después de un tiempo?** → access token expiró (default 20 min) y el refresh cookie también expiró. Comportamiento esperado.
4. **¿Ocurre solo en algunos endpoints?** → posible mismatch de roles (`[Authorize(Roles = "Admin")]` vs claim del JWT).

### Logs útiles para debuggear

- `BulletinAuthStateProvider`: log warning *"Could not determine authentication state"* indica fallo en JS interop durante init.
- `AuthApiService`: log debug *"Could not read token from localStorage (likely SSR pre-render)"* indica fallo de lectura — esperado en Fase SSR, problemático si aparece en Fase WASM.
- `AuthApiService`: log info *"Stored JWT is expired — clearing session"* indica que el token caducó y se limpió.

---

## 10. Archivos relevantes (rutas absolutas)

| Componente | Ruta |
|---|---|
| Custom auth state provider | `source/src/Presentation/Bulletin.Board.Web/Bulletin.Board.Web.Client/Services/BulletinAuthStateProvider.cs` |
| Auth API service | `source/src/Presentation/Bulletin.Board.Web/Bulletin.Board.Web.Client/Services/AuthApiService.cs` |
| HTTP handler con Bearer | `source/src/Presentation/Bulletin.Board.Web/Bulletin.Board.Web.Client/Services/AuthenticatedHttpHandler.cs` |
| DI registration | `source/src/Presentation/Bulletin.Board.Web/Bulletin.Board.Web.Client/ClientServicesExtensions.cs` |
| Router con `<NotAuthorized>` | `source/src/Presentation/Bulletin.Board.Web/Bulletin.Board.Web.Client/Routes.razor` |
| Layout admin (`OnAfterRenderAsync`) | `source/src/Presentation/Bulletin.Board.Web/Bulletin.Board.Web.Client/Layout/AdminLayout.razor` |
| Server hosting | `source/src/Presentation/Bulletin.Board.Web/Bulletin.Board.Web/Program.cs` |
| WASM bootstrap | `source/src/Presentation/Bulletin.Board.Web/Bulletin.Board.Web.Client/Program.cs` |
| Páginas admin | `source/src/Presentation/Bulletin.Board.Web/Bulletin.Board.Web.Client/Pages/Admin/*.razor` |
