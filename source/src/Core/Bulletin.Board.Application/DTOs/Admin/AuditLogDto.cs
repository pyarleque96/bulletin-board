namespace Bulletin.Board.Application.DTOs.Admin;

public record AuditLogDto(
    Guid Id,
    string Action,
    string ResourceType,
    Guid? ResourceId,
    Guid? ActorUserId,
    DateTimeOffset CreatedAt);
