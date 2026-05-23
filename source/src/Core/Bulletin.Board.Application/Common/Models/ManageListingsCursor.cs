using System.Buffers.Text;
using System.Globalization;
using System.Text;

namespace Bulletin.Board.Application.Common.Models;

/// <summary>
/// Opaque base64 cursor for /manage/listings infinity scroll. Encodes the canonical sort
/// tuple: Hierarchy DESC, ProviderTier DESC, AvgRating DESC, CreatedAt DESC, Id DESC.
/// Id is included as the stable tie-breaker — required so two listings with identical sort
/// values cannot duplicate or skip across pages.
/// </summary>
public sealed record ManageListingsCursor(
    int Hierarchy,
    int ProviderTier,
    decimal AvgRating,
    DateTimeOffset CreatedAt,
    Guid Id)
{
    private const char Sep = '|';

    public string Encode()
    {
        var parts = string.Join(Sep,
            Hierarchy.ToString(CultureInfo.InvariantCulture),
            ProviderTier.ToString(CultureInfo.InvariantCulture),
            AvgRating.ToString("0.00", CultureInfo.InvariantCulture),
            CreatedAt.UtcDateTime.ToString("O", CultureInfo.InvariantCulture),
            Id.ToString("N"));
        var bytes = Encoding.UTF8.GetBytes(parts);
        return Convert.ToBase64String(bytes);
    }

    public static bool TryDecode(string? encoded, out ManageListingsCursor? cursor)
    {
        cursor = null;
        if (string.IsNullOrWhiteSpace(encoded)) return false;
        try
        {
            var bytes = Convert.FromBase64String(encoded);
            var raw = Encoding.UTF8.GetString(bytes);
            var parts = raw.Split(Sep);
            if (parts.Length != 5) return false;
            cursor = new ManageListingsCursor(
                int.Parse(parts[0], CultureInfo.InvariantCulture),
                int.Parse(parts[1], CultureInfo.InvariantCulture),
                decimal.Parse(parts[2], CultureInfo.InvariantCulture),
                DateTimeOffset.Parse(parts[3], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                Guid.ParseExact(parts[4], "N"));
            return true;
        }
        catch
        {
            cursor = null;
            return false;
        }
    }
}
