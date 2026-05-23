using Microsoft.Extensions.DependencyInjection;

namespace Bulletin.Board.Web.Client.Components.Listings.Detail;

public static class ListingDetailLayoutExtensions
{
    public static IServiceCollection AddListingDetailLayouts(
        this IServiceCollection services,
        Action<ListingDetailLayoutOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var options = new ListingDetailLayoutOptions();
        configure(options);

        services.AddSingleton(options);
        services.AddSingleton<IListingDetailLayoutResolver, ListingDetailLayoutResolver>();
        return services;
    }
}
