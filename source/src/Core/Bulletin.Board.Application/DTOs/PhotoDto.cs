namespace Bulletin.Board.Application.DTOs;

/// <summary>
/// Photo DTO for the /manage Photos tab and admin moderation.
/// <see cref="Status"/> is a string ("Public" | "Pending" | "Hidden" | "Rejected") for
/// frontend stability — backend nunca expone el enum tipado.
/// </summary>
public record PhotoDto(
    Guid Id,
    string Url,
    string ThumbnailUrl,
    string? AltEn,
    string? AltEs,
    string Status,
    int Position,
    string? GmFeedback,
    DateTimeOffset UploadedAt);
