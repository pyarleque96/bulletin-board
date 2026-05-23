namespace Bulletin.Board.Application.DTOs;

public record InquiryDto(
    Guid Id,
    string ClientName,
    string ClientWhatsAppPhone,
    string ClientLanguage,
    string Subject,
    string MessagePreview,
    DateTimeOffset CreatedAt,
    bool IsRead,
    bool ContactLogged,
    Guid? RelatedEntityId);
