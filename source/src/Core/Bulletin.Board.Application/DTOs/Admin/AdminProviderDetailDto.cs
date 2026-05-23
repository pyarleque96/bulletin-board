namespace Bulletin.Board.Application.DTOs.Admin;

public record AdminProviderDetailDto(
    Guid Id,
    Guid UserId,
    string Tier,
    string VerificationStatus,
    string WhatsAppNumber,
    string? Bio,
    int Hierarchy,
    DateTimeOffset CreatedAt,
    DateTimeOffset? VerifiedAt,
    bool PhoneVerified,
    bool IdentityVerified,
    bool SocialVerified,
    IReadOnlyList<AdminListingDto> Listings);
