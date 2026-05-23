using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Exceptions;

namespace Bulletin.Board.Domain.Entities;

public class Provider
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public ProviderTier Tier { get; private set; } = ProviderTier.Regular;
    public VerificationStatus VerificationStatus { get; private set; } = VerificationStatus.Pending;
    public DateTimeOffset? VerifiedAt { get; private set; }
    public string? Bio { get; private set; }
    public string WhatsAppNumber { get; private set; } = string.Empty;
    public int Hierarchy { get; private set; } = 0;
    public DateTimeOffset? VipUntil { get; private set; }
    public bool VipIsIndefinite { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public Verification? Verification { get; private set; }
    public ICollection<Listing> Listings { get; private set; } = [];
    public ICollection<Contact> Contacts { get; private set; } = [];
    public ICollection<Rating> Ratings { get; private set; } = [];

    private Provider() { }

    public static Provider Create(Guid userId, string whatsAppNumber, string? bio = null)
        => new() { UserId = userId, WhatsAppNumber = whatsAppNumber, Bio = bio };

    public void ApproveTier(ProviderTier newTier)
    {
        if (newTier is ProviderTier.VIP && VerificationStatus != VerificationStatus.Approved)
            throw new InvalidOperationException("Provider must be Verified before becoming VIP.");

        Tier = newTier;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ApproveVerification()
    {
        VerificationStatus = VerificationStatus.Approved;
        VerifiedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetHierarchy(int value)
    {
        if (value < 0)
            throw new DomainException("Hierarchy value must be zero or greater.");

        Hierarchy = value;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void GrantVip(DateTimeOffset? until, bool indefinite, DateTimeOffset now)
    {
        if (VerificationStatus != VerificationStatus.Approved)
            throw new InvalidOperationException("Provider must be Verified before becoming VIP.");

        if (indefinite == until.HasValue)
            throw new DomainException("Specify either an expiration date or indefinite, not both.");

        if (until.HasValue && until.Value <= now)
            throw new DomainException("VIP expiration must be in the future.");

        Tier = ProviderTier.VIP;
        VipIsIndefinite = indefinite;
        VipUntil = indefinite ? null : until;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RevokeVip()
    {
        VipIsIndefinite = false;
        VipUntil = null;
        Tier = VerificationStatus == VerificationStatus.Approved
            ? ProviderTier.Verified
            : ProviderTier.Regular;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public bool IsVipNow(DateTimeOffset now)
        => Tier == ProviderTier.VIP
           && (VipIsIndefinite || (VipUntil.HasValue && VipUntil.Value > now));
}
