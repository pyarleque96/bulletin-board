namespace Bulletin.Board.Application.DTOs.Admin;

public record AdminImageDto(
    Guid Id,
    Guid ListingId,
    string FilePath,
    bool IsPublic,
    int DisplayOrder,
    DateTimeOffset UploadedAt);
