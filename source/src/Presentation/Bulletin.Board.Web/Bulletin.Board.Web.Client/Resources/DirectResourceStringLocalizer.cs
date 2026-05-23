using Microsoft.Extensions.Localization;
using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace Bulletin.Board.Web.Client.Resources;

/// <summary>
/// IStringLocalizer that reads directly from the satellite assembly's manifest
/// resource stream via ResourceReader, bypassing ResourceManager.GetSatelliteAssembly
/// which does not work reliably in Blazor WASM (mono runtime).
///
/// Flow:
///  1. Read CultureInfo.CurrentUICulture.TwoLetterISOLanguageName ("es").
///  2. Search AppDomain.CurrentDomain.GetAssemblies() for the satellite that
///     was loaded via Assembly.Load in Program.cs.
///  3. Open the manifest resource stream directly from the satellite.
///  4. Build a string→string dictionary (cached per culture).
///  5. Fall back to the neutral (English) assembly if satellite not found.
/// </summary>
public sealed class DirectResourceStringLocalizer<T> : IStringLocalizer<T>
{
    private const string ResourceStreamName =
        "Bulletin.Board.Web.Client.Resources.SharedResource.resources";

    private static readonly Assembly MainAssembly = typeof(T).Assembly;

    private static readonly ConcurrentDictionary<string, IReadOnlyDictionary<string, string>> Cache = new();

    public LocalizedString this[string name]
    {
        get
        {
            var dict = GetDictionary(CultureInfo.CurrentUICulture);
            if (dict.TryGetValue(name, out var value))
                return new LocalizedString(name, value, resourceNotFound: false);

            // English neutral fallback
            var enDict = GetDictionary(new CultureInfo("en"));
            if (enDict.TryGetValue(name, out value))
                return new LocalizedString(name, value, resourceNotFound: false);

            return new LocalizedString(name, name, resourceNotFound: true);
        }
    }

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            var ls = this[name];
            return new LocalizedString(
                name,
                string.Format(CultureInfo.CurrentUICulture, ls.Value, arguments),
                ls.ResourceNotFound);
        }
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        var dict = GetDictionary(CultureInfo.CurrentUICulture);
        return dict.Select(kv => new LocalizedString(kv.Key, kv.Value, resourceNotFound: false));
    }

    private static IReadOnlyDictionary<string, string> GetDictionary(CultureInfo culture)
        => Cache.GetOrAdd(culture.TwoLetterISOLanguageName, LoadStrings);

    private static IReadOnlyDictionary<string, string> LoadStrings(string twoLetterName)
    {
        var assembly = ResolveAssembly(twoLetterName);
        using var stream = assembly.GetManifestResourceStream(ResourceStreamName);
        if (stream is null)
            return new Dictionary<string, string>(0);

        var dict = new Dictionary<string, string>(StringComparer.Ordinal);
        using var reader = new ResourceReader(stream);
        var enumerator = reader.GetEnumerator();
        while (enumerator.MoveNext())
        {
            if (enumerator.Key is string key && enumerator.Value is string val)
                dict[key] = val;
        }
        return dict;
    }

    private static Assembly ResolveAssembly(string twoLetterName)
    {
        if (twoLetterName.Equals("en", StringComparison.OrdinalIgnoreCase))
            return MainAssembly;

        // 1. Look for the satellite already loaded by Assembly.Load in Program.cs.
        //    AppDomain.CurrentDomain.GetAssemblies() includes all assemblies loaded
        //    by Assembly.Load, which is what Program.cs uses.
        var loaded = AppDomain.CurrentDomain
            .GetAssemblies()
            .FirstOrDefault(a =>
                string.Equals(a.GetName().Name, "Bulletin.Board.Web.Client.resources",
                    StringComparison.OrdinalIgnoreCase) &&
                string.Equals(
                    a.GetName().CultureInfo?.TwoLetterISOLanguageName,
                    twoLetterName,
                    StringComparison.OrdinalIgnoreCase));

        if (loaded is not null)
            return loaded;

        // 2. Fallback: try Assembly.GetSatelliteAssembly (works on server-side renders).
        try
        {
            return MainAssembly.GetSatelliteAssembly(new CultureInfo(twoLetterName));
        }
        catch
        {
            return MainAssembly;
        }
    }
}
