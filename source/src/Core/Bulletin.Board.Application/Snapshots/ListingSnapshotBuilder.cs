using System.Text.Json;
using Bulletin.Board.Domain.Entities;

namespace Bulletin.Board.Application.Snapshots;

internal sealed class ListingSnapshotBuilder : IListingSnapshotBuilder
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        // Camel-case so the JSON column is friendly to read with psql / pgAdmin.
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        // Don't emit nulls — keeps the JSONB row size down.
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    public string Build(Listing listing)
    {
        ArgumentNullException.ThrowIfNull(listing);

        var snapshot = new ListingSnapshot(
            Version: ListingSnapshot.CurrentVersion,
            SnapshotAt: DateTimeOffset.UtcNow,
            TitleEn: listing.TitleEn,
            TitleEs: listing.TitleEs,
            DescriptionEn: listing.DescriptionEn,
            DescriptionEs: listing.DescriptionEs,
            Price: listing.Price,
            PriceLabelEn: listing.PriceLabelEn,
            PriceLabelEs: listing.PriceLabelEs,
            Location: listing.Location,
            WhatsAppNumber: listing.WhatsAppNumber,
            Images: listing.Images
                .Where(i => i.IsPublic)
                .OrderBy(i => i.DisplayOrder)
                .Select(i => new SnapshotImage(i.Id, i.FilePath, i.DisplayOrder))
                .ToArray(),
            Vehicles: listing.Vehicles
                .Where(v => v.IsActive)
                .OrderBy(v => v.DisplayOrder)
                .ThenBy(v => v.CreatedAt)
                .Select(v => new SnapshotVehicle(
                    Id: v.Id,
                    Name: v.Name,
                    PassengerMax: v.PassengerMax,
                    Transmission: v.Transmission.ToString(),
                    HasAirConditioning: v.HasAirConditioning,
                    DailyRateCents: v.DailyRateCents,
                    WeeklyRateCents: v.WeeklyRateCents,
                    MonthlyRateCents: v.MonthlyRateCents,
                    DisplayOrder: v.DisplayOrder,
                    Photos: v.Photos
                        .Where(p => p.IsPublic)
                        .OrderByDescending(p => p.IsPrimary)
                        .ThenBy(p => p.DisplayOrder)
                        .Select(p => new SnapshotVehiclePhoto(
                            p.Id, p.FilePath, p.IsPrimary, p.DisplayOrder))
                        .ToArray()))
                .ToArray());

        return JsonSerializer.Serialize(snapshot, SerializerOptions);
    }

    public ListingSnapshot? Parse(string? snapshotJson)
    {
        if (string.IsNullOrWhiteSpace(snapshotJson))
            return null;

        try
        {
            return JsonSerializer.Deserialize<ListingSnapshot>(snapshotJson, SerializerOptions);
        }
        catch (JsonException)
        {
            // Caller logs; we don't have ILogger here without breaking purity. Returning null
            // forces the read path to fall back to live data, which is the safe behavior.
            return null;
        }
    }
}
