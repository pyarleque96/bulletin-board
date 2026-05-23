namespace Bulletin.Board.Web.Client.Components.Listings.Detail;

internal sealed class ListingDetailLayoutResolver(ListingDetailLayoutOptions options)
    : IListingDetailLayoutResolver
{
    public Type Resolve(string categorySlug, string tier)
    {
        var category = string.IsNullOrWhiteSpace(categorySlug) ? ListingDetailLayoutOptions.AnyCategory : categorySlug;
        var t        = string.IsNullOrWhiteSpace(tier)         ? ListingDetailLayoutOptions.AnyTier     : tier;

        // Order matters — most specific first.
        var lookups = new (string Category, string Tier)[]
        {
            (category, t),
            (category, ListingDetailLayoutOptions.AnyTier),
            (ListingDetailLayoutOptions.AnyCategory, t),
            (ListingDetailLayoutOptions.AnyCategory, ListingDetailLayoutOptions.AnyTier),
        };

        foreach (var key in lookups)
        {
            if (options.Registrations.TryGetValue(key, out var match))
                return match;
        }

        return options.Fallback
            ?? throw new InvalidOperationException(
                "No listing detail layout matched and no fallback was registered. " +
                "Call options.RegisterFallback(typeof(GenericDetail)) at startup.");
    }
}
