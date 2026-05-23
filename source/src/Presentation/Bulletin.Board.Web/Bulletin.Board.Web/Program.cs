using Bulletin.Board.Web.Client;
using Bulletin.Board.Web.Components;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// Localization (EN / ES)
// ============================================================
builder.Services.AddLocalization(options =>
    options.ResourcesPath = "Resources");

// ============================================================
// Blazor with both interactive render modes
// ============================================================
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// ============================================================
// Authentication — required by AuthorizeRouteView in the server pipeline
// even when prerender is disabled. The real auth is JWT handled client-side
// by BulletinAuthStateProvider; the cookie scheme here just satisfies the
// IAuthenticationService dependency.
// ============================================================
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath  = "/login";
        options.LogoutPath = "/logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.SlidingExpiration = false;

        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization();

// ============================================================
// Client services — needed for SSR pre-rendering with InteractiveAuto
// ============================================================
var serverBaseUrl = builder.Configuration["Api:BaseUrl"] ?? "https://localhost:7207";
builder.Services.AddClientServices(serverBaseUrl);

var app = builder.Build();

// ============================================================
// Localization middleware
// ============================================================
var supportedCultures = new[] { "en", "es" };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en"),
    SupportedCultures = supportedCultures.Select(c => new CultureInfo(c)).ToList(),
    SupportedUICultures = supportedCultures.Select(c => new CultureInfo(c)).ToList(),
    RequestCultureProviders =
    [
        new CookieRequestCultureProvider(),
        new QueryStringRequestCultureProvider { QueryStringKey = "lang", UIQueryStringKey = "lang" },
        new AcceptLanguageHeaderRequestCultureProvider(),
    ]
});

// ============================================================
// HTTP pipeline
// ============================================================
app.UseExceptionHandler("/Error", createScopeForErrors: true);

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.UseStatusCodePagesWithRedirects("/not-found");

app.MapStaticAssets();

// ============================================================
// Culture switch endpoint
// ============================================================
app.MapGet("/Culture/Set", (HttpContext httpContext, string culture, string redirectUri) =>
{
    var supported = new[] { "en", "es" };
    if (!supported.Contains(culture))
        culture = "en";

    var cookieValue = CookieRequestCultureProvider.MakeCookieValue(
        new RequestCulture(culture, culture));

    httpContext.Response.Cookies.Append(
        CookieRequestCultureProvider.DefaultCookieName,
        cookieValue,
        new CookieOptions
        {
            MaxAge      = TimeSpan.FromDays(365),
            IsEssential = true,
            Path        = "/",
            SameSite    = SameSiteMode.Lax,
        });

    var uri = Uri.TryCreate(redirectUri, UriKind.RelativeOrAbsolute, out var parsed) && !parsed.IsAbsoluteUri
        ? redirectUri
        : "/";

    return Results.Redirect(uri, permanent: false);
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Bulletin.Board.Web.Client._Imports).Assembly)
    .AllowAnonymous();

app.Run();
