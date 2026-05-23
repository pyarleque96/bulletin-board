namespace Bulletin.Board.Application.DTOs.Admin;

public record AdminProviderDto(
    Guid Id,
    Guid UserId,
    string? FirstName,
    string? LastName,
    string Tier,
    string VerificationStatus,
    string WhatsAppNumber,
    string? Bio,
    int Hierarchy,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    int ListingsCount,
    bool IsVipNow,
    DateTimeOffset? VipUntil,
    bool VipIsIndefinite,
    bool PhoneVerified,
    bool IdVerified,
    bool SocialVerified);
