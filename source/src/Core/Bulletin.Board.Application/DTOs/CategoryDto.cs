namespace Bulletin.Board.Application.DTOs;

public record CategoryDto(
    Guid Id,
    string NameEn,
    string NameEs,
    string Slug,
    string? Icon,
    int? ListingCount = null);
