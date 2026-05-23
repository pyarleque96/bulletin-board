namespace Bulletin.Board.Web.Client.Components.Manage;

internal sealed class ManageLayoutResolver(ManageLayoutOptions options)
    : IManageLayoutResolver
{
    public Type Resolve(string categorySlug, string tier)
    {
        var category = string.IsNullOrWhiteSpace(categorySlug) ? ManageLayoutOptions.AnyCategory : categorySlug;
        var t        = string.IsNullOrWhiteSpace(tier)         ? ManageLayoutOptions.AnyTier     : tier;

        // Normalise tier: anything that isn't "VIP" is treated as Regular (Regular + Verified
        // share the same management panel — VIP-only features are gated inside each layout).
        // The mockups only define two variants per category: Regular and VIP.
        var normalisedTier = string.Equals(t, "VIP", StringComparison.OrdinalIgnoreCase) ? "VIP" : "Regular";

        // Order matters — most specific first.
        var lookups = new (string Category, string Tier)[]
        {
            (category, normalisedTier),
            (category, ManageLayoutOptions.AnyTier),
            (ManageLayoutOptions.AnyCategory, normalisedTier),
            (ManageLayoutOptions.AnyCategory, ManageLayoutOptions.AnyTier),
        };

        foreach (var key in lookups)
        {
            if (options.Registrations.TryGetValue(key, out var match))
                return match;
        }

        return options.Fallback
            ?? throw new InvalidOperationException(
                "No manage layout matched and no fallback was registered. " +
                "Call options.RegisterFallback(typeof(GenericManageRegular)) at startup.");
    }
}
