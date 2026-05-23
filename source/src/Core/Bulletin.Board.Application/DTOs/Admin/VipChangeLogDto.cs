namespace Bulletin.Board.Application.DTOs.Admin;

public record VipChangeLogDto(
    Guid Id,
    string EntityType,
    Guid EntityId,
    string Action,
    DateTimeOffset? PreviousVipUntil,
    bool PreviousIsIndefinite,
    DateTimeOffset? NewVipUntil,
    bool NewIsIndefinite,
    Guid ChangedByUserId,
    DateTimeOffset ChangedAt,
    string? IpAddress,
    string? Reason);
