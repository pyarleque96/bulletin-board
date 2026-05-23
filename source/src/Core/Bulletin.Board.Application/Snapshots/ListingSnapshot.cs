namespace Bulletin.Board.Application.Snapshots;

/// <summary>
/// Frozen, GM-approved view of a listing's editable content. Persisted as JSONB on
/// <c>listings.published_snapshot</c>.  Public reads (<c>GetListingDetailBySlug</c>,
/// search) project from this snapshot so a listing under re-review keeps showing the
/// last approved version while the provider edits.
/// </summary>
/// <remarks>
/// Reviews, ratings, similar listings and provider verification are NOT in the snapshot.
/// They are always read live because they change independently of provider edits.
/// <para>
/// <see cref="Version"/> is bumped when this record's shape changes so consumers can
/// migrate older JSON. v1 = initial release. v2 = removed SnapshotVehicle.PassengerMin.
/// </para>
/// </remarks>
public sealed record ListingSnapshot(
    int Version,
    DateTimeOffset SnapshotAt,
    string TitleEn,
    string TitleEs,
    string? DescriptionEn,
    string? DescriptionEs,
    decimal? Price,
    string? PriceLabelEn,
    string? PriceLabelEs,
    string? Location,
    string? WhatsAppNumber,
    SnapshotImage[] Images,
    SnapshotVehicle[] Vehicles)
{
    public const int CurrentVersion = 2;
}

public sealed record SnapshotImage(
    Guid Id,
    string FilePath,
    int DisplayOrder);

public sealed record SnapshotVehicle(
    Guid Id,
    string Name,
    int PassengerMax,
    string Transmission,
    bool HasAirConditioning,
    int DailyRateCents,
    int? WeeklyRateCents,
    int? MonthlyRateCents,
    int DisplayOrder,
    SnapshotVehiclePhoto[] Photos);

public sealed record SnapshotVehiclePhoto(
    Guid Id,
    string FilePath,
    bool IsPrimary,
    int DisplayOrder);
