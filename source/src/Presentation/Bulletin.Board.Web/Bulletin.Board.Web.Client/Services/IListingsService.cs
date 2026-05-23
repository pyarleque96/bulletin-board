using Bulletin.Board.Web.Client.Models;

namespace Bulletin.Board.Web.Client.Services;

/// <summary>
/// Contract for listing-related API calls.
/// All methods return null on 404; throw on 5xx.
/// </summary>
public interface IListingsService
{
    /// <summary>
    /// Returns the top N approved listings for the home page, ordered by canonical ranking.
    /// </summary>
    Task<IReadOnlyList<ListingCardDto>?> GetTopAsync(int count = 10, CancellationToken ct = default);

    Task<PagedResult<ListingCardDto>?> SearchAsync(
        string?  category    = null,
        string?  tier        = null,
        decimal? minRating   = null,
        string?  language    = null,
        int      page        = 1,
        int      pageSize    = 12,
        CancellationToken ct = default);

    Task<ListingDetailDto?> GetDetailAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Same as <see cref="GetDetailAsync(Guid, CancellationToken)"/> but keyed by the public slug.
    /// Used by the SEO-friendly route <c>/listing/detail/{slug}</c>.
    /// </summary>
    Task<ListingDetailDto?> GetDetailBySlugAsync(string slug, CancellationToken ct = default);

    /// <summary>
    /// Initiates contact with a provider. Returns a WhatsApp deep-link and waiver info.
    /// Requires authentication. Returns null if user is not authenticated.
    /// </summary>
    Task<ContactResultDto?> ContactProviderAsync(Guid listingId, CancellationToken ct = default);

    // ─── Vehicle management (owner) ────────────────────────────────────────────

    Task<IReadOnlyList<OwnerVehicleDto>?> GetOwnerVehiclesAsync(Guid listingId, CancellationToken ct = default);

    Task<Guid?> CreateVehicleAsync(Guid listingId, CreateVehicleRequest request, CancellationToken ct = default);

    Task<bool> UpdateVehicleAsync(Guid vehicleId, UpdateVehicleRequest request, CancellationToken ct = default);

    Task<bool> DeleteVehicleAsync(Guid vehicleId, CancellationToken ct = default);

    Task<bool> SetVehicleActiveAsync(Guid vehicleId, bool isActive, CancellationToken ct = default);

    // ─── Vehicle photos ────────────────────────────────────────────────────────

    Task<Guid?> AddVehiclePhotoAsync(Guid vehicleId, AddVehiclePhotoRequest request, CancellationToken ct = default);

    /// <summary>
    /// Uploads a photo file via multipart/form-data; the API stores it in blob storage and
    /// creates the VehiclePhoto row pointing at the resulting URL.
    /// </summary>
    Task<Guid?> UploadVehiclePhotoAsync(
        Guid vehicleId, Stream content, string fileName, string contentType,
        int displayOrder, bool isPrimary, CancellationToken ct = default);

    Task<bool> DeleteVehiclePhotoAsync(Guid photoId, CancellationToken ct = default);

    Task<bool> SetVehiclePhotoPrimaryAsync(Guid photoId, CancellationToken ct = default);

    // ─── Vehicle availability ──────────────────────────────────────────────────

    /// <summary>Returns blackout ranges intersecting [from, to]. Public; no auth required.</summary>
    Task<VehicleAvailabilityDto?> GetVehicleAvailabilityAsync(
        Guid vehicleId, DateOnly from, DateOnly to, CancellationToken ct = default);

    /// <summary>Adds a blackout. Owner only. Returns null on conflict (overlap) or auth failure.</summary>
    Task<Guid?> AddVehicleUnavailabilityAsync(
        Guid vehicleId, AddUnavailabilityRequest request, CancellationToken ct = default);

    /// <summary>Deletes a blackout. Owner only.</summary>
    Task<bool> DeleteVehicleUnavailabilityAsync(Guid unavailabilityId, CancellationToken ct = default);

    // ─── Reservations ─────────────────────────────────────────────────────────

    /// <summary>Submits a reservation. Returns null on auth failure or conflict.</summary>
    Task<Guid?> CreateReservationAsync(
        Guid vehicleId, CreateReservationRequest request, CancellationToken ct = default);

    /// <summary>Owner panel: reservations for a listing, optionally filtered by status.</summary>
    Task<IReadOnlyList<ReservationRequestDto>?> GetListingReservationsAsync(
        Guid listingId, string? status = null, CancellationToken ct = default);

    /// <summary>Requester history: reservations submitted by the current user.</summary>
    Task<IReadOnlyList<MyReservationDto>?> GetMyReservationsAsync(CancellationToken ct = default);

    Task<bool> AcceptReservationAsync(Guid reservationId, string? note, CancellationToken ct = default);
    Task<bool> RejectReservationAsync(Guid reservationId, string? note, CancellationToken ct = default);
    Task<bool> CancelReservationAsync(Guid reservationId, string? note, CancellationToken ct = default);

    /// <summary>Listings owned by the current user. Returns null on auth failure.</summary>
    Task<IReadOnlyList<MyListingDto>?> GetMyListingsAsync(CancellationToken ct = default);
}
