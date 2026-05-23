using Bulletin.Board.Web.Client.Components.Listings.Detail;
using Bulletin.Board.Web.Client.Components.Listings.Detail.Layouts.CarRental;
using Bulletin.Board.Web.Client.Components.Listings.Detail.Layouts.Generic;
using Bulletin.Board.Web.Client.Components.Manage;
using Bulletin.Board.Web.Client.Components.Manage.Layouts.Beauty;
using Bulletin.Board.Web.Client.Components.Manage.Layouts.CarRental;
using Bulletin.Board.Web.Client.Components.Manage.Layouts.Generic;
using Bulletin.Board.Web.Client.Components.Manage.Layouts.Housing;
using Bulletin.Board.Web.Client.Components.Manage.Layouts.Jobs;
using Bulletin.Board.Web.Client.Components.Manage.Layouts.Mechanics;
using Bulletin.Board.Web.Client.Components.Manage.Layouts.Transportation;
using Bulletin.Board.Web.Client.Resources;
using Bulletin.Board.Web.Client.Services;
using Bulletin.Board.Web.Client.Services.Http;
using Bulletin.Board.Web.Client.Services.State;
using Bulletin.Board.Web.Client.States;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;

namespace Bulletin.Board.Web.Client;

public static class ClientServicesExtensions
{
    public static IServiceCollection AddClientServices(
        this IServiceCollection services,
        string apiBaseUrl)
    {
        // Localization
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        services.AddSingleton<IStringLocalizer<SharedResource>, DirectResourceStringLocalizer<SharedResource>>();

        // LocalStorageService is kept for potential future use by other features,
        // but it is NO LONGER used for token storage (tokens live in memory only).
        services.AddScoped<LocalStorageService>();

        // ── Named HTTP client for unauthenticated calls (login, register, refresh).
        // In Blazor WASM, the HttpClient is backed by the browser's fetch API, which
        // automatically includes same-site cookies (including the HttpOnly refresh-token
        // cookie) without any extra configuration. No custom HttpClientHandler is needed.
        services.AddHttpClient("auth", client => client.BaseAddress = new Uri(apiBaseUrl));

        // AuthApiService — token is stored in memory only (never in localStorage).
        // Scoped lifetime means the token cache is shared within one browser tab.
        services.AddScoped<AuthApiService>(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            var client  = factory.CreateClient("auth");
            var logger  = sp.GetRequiredService<ILogger<AuthApiService>>();
            var js      = sp.GetRequiredService<IJSRuntime>();
            return new AuthApiService(client, logger, js);
        });
        services.AddScoped<IAuthService>(sp => sp.GetRequiredService<AuthApiService>());

        // JWT handler for outbound API calls.
        // AuthenticatedHttpHandler now also requires NavigationManager so it can
        // redirect to /login?reason=expired when a refresh fails.
        services.AddTransient<AuthenticatedHttpHandler>();

        // Typed HTTP clients protected by the JWT handler.
        services
            .AddHttpClient<IListingsService, ListingsApiService>(client =>
                client.BaseAddress = new Uri(apiBaseUrl))
            .AddHttpMessageHandler<AuthenticatedHttpHandler>();

        services
            .AddHttpClient<ICategoriesService, CategoriesApiService>(client =>
                client.BaseAddress = new Uri(apiBaseUrl))
            .AddHttpMessageHandler<AuthenticatedHttpHandler>();

        services
            .AddHttpClient<IAdminService, AdminApiService>(client =>
                client.BaseAddress = new Uri(apiBaseUrl))
            .AddHttpMessageHandler<AuthenticatedHttpHandler>();

        // ── Manage-panel services (F2) ─────────────────────────────────────────
        services
            .AddHttpClient<IListingService, ListingService>(client =>
                client.BaseAddress = new Uri(apiBaseUrl))
            .AddHttpMessageHandler<AuthenticatedHttpHandler>();

        services
            .AddHttpClient<IVehicleService, VehicleService>(client =>
                client.BaseAddress = new Uri(apiBaseUrl))
            .AddHttpMessageHandler<AuthenticatedHttpHandler>();

        services
            .AddHttpClient<IReservationService, ReservationService>(client =>
                client.BaseAddress = new Uri(apiBaseUrl))
            .AddHttpMessageHandler<AuthenticatedHttpHandler>();

        services
            .AddHttpClient<IReviewService, ReviewService>(client =>
                client.BaseAddress = new Uri(apiBaseUrl))
            .AddHttpMessageHandler<AuthenticatedHttpHandler>();

        services
            .AddHttpClient<IContactLogService, ContactLogService>(client =>
                client.BaseAddress = new Uri(apiBaseUrl))
            .AddHttpMessageHandler<AuthenticatedHttpHandler>();

        // Manage state container — scoped so detail sub-pages share listing cache.
        services.AddScoped<ManageStateService>();

        // Authorization & authentication state.
        services.AddAuthorizationCore();
        services.AddScoped<BulletinAuthStateProvider>();
        services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<BulletinAuthStateProvider>());

        // UI States.
        services.AddScoped<FooterState>();
        services.AddScoped<NavBarState>();
        services.AddScoped<AdminNavigationState>();

        // Listing detail layouts. Resolution order in IListingDetailLayoutResolver:
        // (cat, tier) -> (cat, *) -> (*, tier) -> fallback.
        services.AddListingDetailLayouts(opts =>
        {
            // Car rental: VIP gets carousel + Vehicles section. Verified/Regular get Basic.
            opts.Register("car-rental", "VIP", typeof(CarRentalVipDetail));
            opts.Register("car-rental", ListingDetailLayoutOptions.AnyTier, typeof(CarRentalBasicDetail));

            opts.RegisterFallback(typeof(GenericDetail));
        });

        // Manage panels (owner-facing /manage/{id}). Same resolution order as the
        // public detail layouts, but here tiers normalise to Regular | VIP since
        // the mockups only define two variants per category.
        services.AddManageLayouts(opts =>
        {
            // Beauty
            opts.Register("beauty",         "VIP",     typeof(BeautyManageVip));
            opts.Register("beauty",         "Regular", typeof(BeautyManageRegular));

            // Car rental — Fase D. The /manage/{id} dispatcher in ManageDetailOverview
            // recognises car-rental and delegates to the resolver, instead of keeping
            // the inline UI. The sub-routes /vehicles and /reservations remain valid
            // because they are separate pages that ALSO delegate via the resolver
            // when they detect a non-car-rental (or in this case, ANY) category.
            opts.Register("car-rental",     "VIP",     typeof(CarRentalManageVip));
            opts.Register("car-rental",     "Regular", typeof(CarRentalManageRegular));

            // Housing
            opts.Register("housing",        "VIP",     typeof(HousingManageVip));
            opts.Register("housing",        "Regular", typeof(HousingManageRegular));

            // Jobs
            opts.Register("jobs",           "VIP",     typeof(JobsManageVip));
            opts.Register("jobs",           "Regular", typeof(JobsManageRegular));

            // Mechanics (slug "mechanic" matches the Manage.razor filter dropdown)
            opts.Register("mechanic",       "VIP",     typeof(MechanicsManageVip));
            opts.Register("mechanic",       "Regular", typeof(MechanicsManageRegular));
            opts.Register("mechanics",      "VIP",     typeof(MechanicsManageVip));
            opts.Register("mechanics",      "Regular", typeof(MechanicsManageRegular));

            // Transportation — both "transport" (legacy) and "transportation" (mockup-aligned)
            opts.Register("transport",      "VIP",     typeof(TransportationManageVip));
            opts.Register("transport",      "Regular", typeof(TransportationManageRegular));
            opts.Register("transportation", "VIP",     typeof(TransportationManageVip));
            opts.Register("transportation", "Regular", typeof(TransportationManageRegular));

            // Generic fallback for unregistered categories.
            opts.Register(ManageLayoutOptions.AnyCategory, "VIP",     typeof(GenericManageVip));
            opts.Register(ManageLayoutOptions.AnyCategory, "Regular", typeof(GenericManageRegular));
            opts.RegisterFallback(typeof(GenericManageRegular));
        });

        return services;
    }
}
