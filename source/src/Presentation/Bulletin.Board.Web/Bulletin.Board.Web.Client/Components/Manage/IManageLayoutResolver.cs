namespace Bulletin.Board.Web.Client.Components.Manage;

/// <summary>
/// Resolves which Blazor component renders the <c>/manage/{id}</c> detail panel
/// for a given listing, based on its category slug and provider tier.
/// </summary>
/// <remarks>
/// Mirrors <see cref="Bulletin.Board.Web.Client.Components.Listings.Detail.IListingDetailLayoutResolver"/>
/// but for the owner-facing management panels. Each category gets a Regular variant
/// (Regular + Verified tiers) and a VIP variant. Unregistered categories fall back
/// to the generic layout registered via <c>RegisterFallback</c>.
///
/// Resolution order (first match wins):
/// 1. (categorySlug, tier) exact pair.
/// 2. (categorySlug, "*") — same category, any tier.
/// 3. ("*", tier) — any category, same tier.
/// 4. Fallback layout registered via <c>RegisterFallback</c>.
/// </remarks>
public interface IManageLayoutResolver
{
    Type Resolve(string categorySlug, string tier);
}
