using Microsoft.Extensions.DependencyInjection;

namespace Bulletin.Board.Web.Client.Components.Manage;

public static class ManageLayoutExtensions
{
    public static IServiceCollection AddManageLayouts(
        this IServiceCollection services,
        Action<ManageLayoutOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var options = new ManageLayoutOptions();
        configure(options);

        services.AddSingleton(options);
        services.AddSingleton<IManageLayoutResolver, ManageLayoutResolver>();
        return services;
    }
}
