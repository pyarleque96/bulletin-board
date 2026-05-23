using Bulletin.Board.Domain.Common;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Events;
using Bulletin.Board.Domain.Exceptions;

namespace Bulletin.Board.Domain.Entities;

public class Listing
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ProviderId { get; private set; }
    public Guid CategoryId { get; private set; }
    public string Slug { get; private set; } = string.Empty;
    public string TitleEn { get; private set; } = string.Empty;
    public string TitleEs { get; private set; } = string.Empty;
    public string? DescriptionEn { get; private set; }
    public string? DescriptionEs { get; private set; }
    public decimal? Price { get; private set; }
    public string? PriceLabelEn { get; private set; }
    public string? PriceLabelEs { get; private set; }
    public string? Location { get; private set; }
    public string? WhatsAppNumber { get; private set; }
    public ListingStatus Status { get; private set; } = ListingStatus.Pending;
    public string? RejectionReason { get; private set; }
    public ProviderTier ProviderTier { get; private set; }
    public DateTimeOffset? VipUntil { get; private set; }
    public bool VipIsIndefinite { get; private set; }
    public decimal? AvgRating { get; private set; }
    public DateTimeOffset? PublishedAt { get; private set; }
    /// <summary>
    /// JSONB blob captured at the last <see cref="Approve"/> call. Public reads project from
    /// this so a listing under re-review keeps showing the last GM-approved version while the
    /// provider edits. Null until the first approval.
    /// </summary>
    public string? PublishedSnapshot { get; private set; }
    public int? PublishedSnapshotVersion { get; private set; }
    // /postgres-best-practices: denormalized column avoids runtime JOIN on sort path,
    // enabling the covering index idx_listings_ranked to be used by the planner.
    public int ProviderHierarchy { get; private set; }
    /// <summary>
    /// Composite ranking score recomputed hourly by TrendingScoreRecalculatorService.
    /// Combines Hierarchy*1000 + Tier*50 + AvgRating*20 + log(1+WhatsApp30d)*30 + recency boost.
    /// Used by /api/v1/listings/top — never edit directly.
    /// </summary>
    public double TrendingScore { get; private set; }
    public DateTimeOffset? ScoreUpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    /// <summary>
    /// Owner-controlled visibility toggle. Paused listings are hidden from public searches
    /// but remain visible in the owner's dashboard (/me/listings). Does not affect Status.
    /// </summary>
    public bool IsPausedByOwner { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public Provider Provider { get; private set; } = null!;
    public Category Category { get; private set; } = null!;
    public ICollection<ListingImage> Images { get; private set; } = [];
    public ICollection<Contact> Contacts { get; private set; } = [];
    public ICollection<Rating> Ratings { get; private set; } = [];
    public ICollection<Vehicle> Vehicles { get; private set; } = [];

    private readonly List<object> _domainEvents = [];
    public IReadOnlyList<object> DomainEvents => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();

    private Listing() { }

    public static Listing Create(Guid providerId, Guid categoryId, ProviderTier providerTier,
        string titleEn, string titleEs, string? descriptionEn = null, string? descriptionEs = null,
        decimal? price = null, string? location = null, string? whatsAppNumber = null,
        string? slug = null)
    {
        if (price is <= 0)
            throw new ArgumentException("Price must be greater than zero.");

        var resolvedSlug = string.IsNullOrWhiteSpace(slug)
            ? SlugGenerator.Slugify(titleEn)
            : SlugGenerator.Slugify(slug);

        return new Listing
        {
            ProviderId = providerId,
            CategoryId = categoryId,
            ProviderTier = providerTier,
            Slug = resolvedSlug,
            TitleEn = titleEn,
            TitleEs = titleEs,
            DescriptionEn = descriptionEn,
            DescriptionEs = descriptionEs,
            Price = price,
            Location = location,
            WhatsAppNumber = whatsAppNumber
        };
    }

    /// <summary>
    /// Replaces the slug. Used by creation/seed flows that need to disambiguate
    /// collisions discovered after construction (e.g. "anuncio" -> "anuncio-2").
    /// Slug is otherwise immutable: there is no public method to change it after publish.
    /// </summary>
    public void OverrideSlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
            throw new DomainException("Slug cannot be empty.");

        Slug = SlugGenerator.Slugify(slug);
    }

    /// <summary>
    /// Replaces the published snapshot in place. Used by the refresher when a non-edit GM action
    /// (e.g. flipping a photo's visibility) needs to update what public reads see, without
    /// changing status or the PublishedAt timestamp.
    /// </summary>
    public void RefreshSnapshot(string snapshotJson, int snapshotVersion)
    {
        if (string.IsNullOrWhiteSpace(snapshotJson))
            throw new DomainException("Snapshot JSON cannot be empty.");
        PublishedSnapshot = snapshotJson;
        PublishedSnapshotVersion = snapshotVersion;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Approve(Guid approverId, string? snapshotJson = null, int? snapshotVersion = null)
    {
        Status = ListingStatus.Approved;
        PublishedAt = DateTimeOffset.UtcNow;
        RejectionReason = null;
        UpdatedAt = DateTimeOffset.UtcNow;

        // Snapshot is optional for backward compat with legacy callers (seed/tests). When the
        // command handler provides one, it freezes the current content so the provider can edit
        // freely afterwards without the public seeing the in-flight changes.
        if (snapshotJson is not null)
        {
            if (string.IsNullOrWhiteSpace(snapshotJson))
                throw new DomainException("Snapshot JSON cannot be empty when provided.");
            PublishedSnapshot = snapshotJson;
            PublishedSnapshotVersion = snapshotVersion ?? 1;
        }

        _domainEvents.Add(new ListingApprovedEvent(Id, ProviderId, approverId));
    }

    public void Reject(string reason)
    {
        Status = ListingStatus.Rejected;
        RejectionReason = reason;
        PublishedAt = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RequestChanges(string feedback)
    {
        Status = ListingStatus.NeedsChanges;
        RejectionReason = feedback;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Edit(string titleEn, string titleEs, string? descriptionEn, string? descriptionEs,
        decimal? price, string? location, string? whatsAppNumber = null)
    {
        TitleEn = titleEn;
        TitleEs = titleEs;
        DescriptionEn = descriptionEn;
        DescriptionEs = descriptionEs;
        Price = price;
        Location = location;
        WhatsAppNumber = whatsAppNumber;
        // Regla #5: cualquier edición del proveedor revierte el listing a Pending.
        Status = ListingStatus.Pending;
        PublishedAt = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Reverts the listing to <see cref="ListingStatus.Pending"/> so the GM re-reviews it.
    /// Called by aggregates of editable children (vehicles, photos, etc.) — regla #5 del
    /// product blueprint: cualquier edicion del proveedor regresa el listing a Pending.
    /// No-op if the listing is already Pending or NeedsChanges.
    /// </summary>
    public void RequireReapproval()
    {
        if (Status == ListingStatus.Approved || Status == ListingStatus.Rejected)
        {
            Status = ListingStatus.Pending;
            PublishedAt = null;
        }
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Pauses the listing so it no longer appears in public searches.
    /// Does NOT change Status — no re-approval required (regla #5 applies only to content edits).
    /// </summary>
    public void PauseByOwner()
    {
        IsPausedByOwner = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Resumes a paused listing so it reappears in public searches.
    /// Does NOT change Status.
    /// </summary>
    public void ResumeByOwner()
    {
        IsPausedByOwner = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SoftDelete() => IsDeleted = true;

    public void UpdateAvgRating(decimal avg)
    {
        AvgRating = avg;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SyncProviderTier(ProviderTier tier) => ProviderTier = tier;

    public void GrantVip(ProviderTier providerTier, DateTimeOffset? until, bool indefinite, DateTimeOffset now)
    {
        if (providerTier != ProviderTier.VIP)
            throw new InvalidOperationException("Listing's provider must be VIP to grant listing VIP.");

        if (indefinite == until.HasValue)
            throw new DomainException("Specify either an expiration date or indefinite, not both.");

        if (until.HasValue && until.Value <= now)
            throw new DomainException("VIP expiration must be in the future.");

        ProviderTier = ProviderTier.VIP;
        VipIsIndefinite = indefinite;
        VipUntil = indefinite ? null : until;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void InheritVipFromProvider(DateTimeOffset? until, bool indefinite)
    {
        ProviderTier = ProviderTier.VIP;
        VipIsIndefinite = indefinite;
        VipUntil = indefinite ? null : until;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RevokeVip(ProviderTier fallbackProviderTier)
    {
        VipIsIndefinite = false;
        VipUntil = null;
        ProviderTier = fallbackProviderTier;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public bool IsVipNow(DateTimeOffset now)
        => ProviderTier == ProviderTier.VIP
           && (VipIsIndefinite || (VipUntil.HasValue && VipUntil.Value > now));
}
