using Bulletin.Board.Web.Client;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using System.Globalization;
using System.Reflection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

var apiBaseUrl = builder.Configuration["Api:BaseUrl"] ?? "https://localhost:7207";

builder.Services.AddClientServices(apiBaseUrl);

var host = builder.Build();

// ── Culture bootstrapping ───────────────────────────────────────────────────
var js = (IJSInProcessRuntime)host.Services.GetRequiredService<IJSRuntime>();
var cultureName = js.Invoke<string>("getCultureFromCookie");

if (!string.IsNullOrWhiteSpace(cultureName))
{
    try
    {
        var culture = new CultureInfo(cultureName);
        CultureInfo.DefaultThreadCurrentCulture   = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        if (!culture.TwoLetterISOLanguageName.Equals("en", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var satelliteAssemblyName = new AssemblyName("Bulletin.Board.Web.Client.resources")
                {
                    CultureInfo = culture
                };
                Assembly.Load(satelliteAssemblyName);
            }
            catch (Exception)
            {
                // Satellite missing — fall back to neutral.
            }
        }
    }
    catch (CultureNotFoundException) { }
}

await host.RunAsync();
