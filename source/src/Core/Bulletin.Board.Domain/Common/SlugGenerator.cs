using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Bulletin.Board.Domain.Common;

public static partial class SlugGenerator
{
    public const int MaxLength = 120;
    private const string FallbackSlug = "listing";

    public static string Slugify(string? text, int maxLength = MaxLength)
    {
        if (maxLength <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxLength));

        if (string.IsNullOrWhiteSpace(text))
            return FallbackSlug;

        var stripped = StripDiacritics(text);
        var lowered = stripped.ToLowerInvariant();
        var alnum = NonAlphanumericRegex().Replace(lowered, "-");
        var collapsed = MultipleDashRegex().Replace(alnum, "-");
        var trimmed = collapsed.Trim('-');

        if (trimmed.Length == 0)
            return FallbackSlug;

        if (trimmed.Length > maxLength)
            trimmed = trimmed[..maxLength].TrimEnd('-');

        return trimmed.Length == 0 ? FallbackSlug : trimmed;
    }

    public static string AppendSuffix(string baseSlug, int suffix, int maxLength = MaxLength)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(baseSlug);
        if (suffix < 1)
            throw new ArgumentOutOfRangeException(nameof(suffix));

        var suffixStr = "-" + suffix.ToString(CultureInfo.InvariantCulture);
        var room = maxLength - suffixStr.Length;
        var head = baseSlug.Length > room ? baseSlug[..room].TrimEnd('-') : baseSlug;
        return head + suffixStr;
    }

    private static string StripDiacritics(string text)
    {
        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);
        foreach (var ch in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    [GeneratedRegex("[^a-z0-9]+", RegexOptions.CultureInvariant)]
    private static partial Regex NonAlphanumericRegex();

    [GeneratedRegex("-{2,}", RegexOptions.CultureInvariant)]
    private static partial Regex MultipleDashRegex();
}
