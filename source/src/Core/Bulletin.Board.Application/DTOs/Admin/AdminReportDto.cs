namespace Bulletin.Board.Application.DTOs.Admin;

public record AdminReportDto(
    Guid Id,
    string Reason,
    string Status,
    Guid ReportedById,
    Guid? ProviderId,
    Guid? ListingId,
    DateTimeOffset CreatedAt);
