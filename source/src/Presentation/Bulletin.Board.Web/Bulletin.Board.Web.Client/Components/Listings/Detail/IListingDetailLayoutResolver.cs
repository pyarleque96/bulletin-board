namespace Bulletin.Board.Web.Client.Components.Listings.Detail;

/// <summary>
/// Resolves which Blazor component renders the detail view of a listing
/// based on its category slug and provider tier.
/// </summary>
/// <remarks>
/// Resolution order (first match wins):
/// 1. (categorySlug, tier) exact pair.
/// 2. (categorySlug, "*") — same category, any tier.
/// 3. ("*", tier) — any category, same tier.
/// 4. Fallback layout registered via <c>RegisterFallback</c>.
///
/// This indirection keeps the detail page free of category/tier branching: each
/// new category brings its own components and only registers them at startup.
/// </remarks>
public interface IListingDetailLayoutResolver
{
    Type Resolve(string categorySlug, string tier);
}
