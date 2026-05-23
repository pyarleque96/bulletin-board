namespace Bulletin.Board.Application.DTOs;

/// <summary>
/// Owner-side summary of a listing. Powers /account/listings y el panel de gestión /manage.
/// Incluye los campos editables para pre-poblar formularios sin round-trip adicional.
/// </summary>
public record MyListingDto(
    Guid Id,
    string Slug,
    string TitleEn,
    string TitleEs,
    string CategorySlug,
    string Status,
    string Tier,
    bool HasPublishedSnapshot,
    bool IsPausedByOwner,
    DateTimeOffset? ApprovedAt,
    DateTimeOffset UpdatedAt,
    // Campos editables — agregados para que el panel /manage pueda pre-poblar el formulario
    // de edición sin un GET adicional al endpoint de detalle.
    string? DescriptionEn,
    string? DescriptionEs,
    decimal? Price,
    string? Location,
    string? WhatsAppNumber);
