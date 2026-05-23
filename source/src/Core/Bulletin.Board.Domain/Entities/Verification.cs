using Bulletin.Board.Domain.Enums;

namespace Bulletin.Board.Domain.Entities;

public class Verification
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ProviderId { get; private set; }
    public string? PhoneNumber { get; private set; }
    public DateTimeOffset? PhoneVerifiedAt { get; private set; }
    public string? PhoneVerificationToken { get; private set; }
    public string? IdentityDocumentEncrypted { get; private set; }
    public VerificationStatus IdentityStatus { get; private set; } = VerificationStatus.Pending;
    public string? SocialProfilesJson { get; private set; }
    public DateTimeOffset? SocialVerifiedAt { get; private set; }
    public VerificationStatus OverallStatus { get; private set; } = VerificationStatus.Pending;
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public Provider Provider { get; private set; } = null!;

    private Verification() { }

    public static Verification Create(Guid providerId)
        => new() { ProviderId = providerId };

    public void VerifyPhone()
    {
        PhoneVerifiedAt = DateTimeOffset.UtcNow;
        PhoneVerificationToken = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ApproveIdentity()
    {
        IdentityStatus = VerificationStatus.Approved;
        UpdatedAt = DateTimeOffset.UtcNow;
        RecalculateOverallStatus();
    }

    public void VerifySocial()
    {
        SocialVerifiedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
        RecalculateOverallStatus();
    }

    private void RecalculateOverallStatus()
    {
        OverallStatus = PhoneVerifiedAt.HasValue
                        && IdentityStatus == VerificationStatus.Approved
                        && SocialVerifiedAt.HasValue
            ? VerificationStatus.Approved
            : VerificationStatus.Pending;
    }
}
