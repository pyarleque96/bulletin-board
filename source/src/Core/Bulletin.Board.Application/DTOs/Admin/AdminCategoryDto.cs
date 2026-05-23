namespace Bulletin.Board.Application.DTOs.Admin;

public record AdminCategoryDto(
    Guid Id,
    string NameEn,
    string NameEs,
    string Slug,
    int DisplayOrder,
    bool IsActive,
    string? Icon);
