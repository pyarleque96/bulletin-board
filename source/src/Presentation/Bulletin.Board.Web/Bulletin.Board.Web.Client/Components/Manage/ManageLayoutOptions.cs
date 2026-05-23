using Microsoft.AspNetCore.Components;

namespace Bulletin.Board.Web.Client.Components.Manage;

/// <summary>
/// Builder collected during DI registration. The resolver consumes this snapshot
/// at runtime — no further registrations after the host is built.
/// </summary>
public sealed class ManageLayoutOptions
{
    public const string AnyCategory = "*";
    public const string AnyTier     = "*";

    private readonly Dictionary<(string Category, string Tier), Type> _registrations = new(LayoutKeyComparer.Instance);
    private Type? _fallback;

    public ManageLayoutOptions Register(string categorySlug, string tier, Type componentType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(categorySlug);
        ArgumentException.ThrowIfNullOrWhiteSpace(tier);
        ArgumentNullException.ThrowIfNull(componentType);
        EnsureIsComponent(componentType);

        _registrations[(categorySlug, tier)] = componentType;
        return this;
    }

    public ManageLayoutOptions RegisterFallback(Type componentType)
    {
        ArgumentNullException.ThrowIfNull(componentType);
        EnsureIsComponent(componentType);
        _fallback = componentType;
        return this;
    }

    internal IReadOnlyDictionary<(string Category, string Tier), Type> Registrations => _registrations;
    internal Type? Fallback => _fallback;

    private static void EnsureIsComponent(Type t)
    {
        if (!typeof(IComponent).IsAssignableFrom(t))
            throw new ArgumentException($"{t.FullName} does not implement IComponent.", nameof(t));
    }

    private sealed class LayoutKeyComparer : IEqualityComparer<(string Category, string Tier)>
    {
        public static readonly LayoutKeyComparer Instance = new();

        public bool Equals((string Category, string Tier) x, (string Category, string Tier) y)
            => string.Equals(x.Category, y.Category, StringComparison.OrdinalIgnoreCase)
            && string.Equals(x.Tier, y.Tier, StringComparison.OrdinalIgnoreCase);

        public int GetHashCode((string Category, string Tier) obj)
            => HashCode.Combine(
                obj.Category.ToLowerInvariant(),
                obj.Tier.ToLowerInvariant());
    }
}
