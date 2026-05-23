using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Exceptions;

namespace Bulletin.Board.Domain.Entities;

/// <summary>
/// Photo attached to a listing. Subject to GM moderation per regla #4:
/// el proveedor sube fotos pero el GM decide cuáles son públicas.
/// </summary>
public class ListingImage
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ListingId { get; private set; }
    public string FilePath { get; private set; } = string.Empty;
    public string? ThumbnailPath { get; private set; }
    public string? AltEn { get; private set; }
    public string? AltEs { get; private set; }

    /// <summary>
    /// Moderation status. Default <see cref="PhotoStatus.Pending"/> at upload.
    /// Public read paths must filter <c>Status == Public</c>.
    /// </summary>
    public PhotoStatus Status { get; private set; } = PhotoStatus.Pending;

    /// <summary>
    /// Optional feedback from the GM when the photo is hidden or rejected.
    /// Shown to the provider in the Photos tab.
    /// </summary>
    public string? GmFeedback { get; private set; }

    public DateTimeOffset? ModeratedAt { get; private set; }
    public Guid? ModeratedByAdminId { get; private set; }

    public int DisplayOrder { get; private set; }
    public DateTimeOffset UploadedAt { get; private set; } = DateTimeOffset.UtcNow;

    public Listing Listing { get; private set; } = null!;

    /// <summary>
    /// Derived flag preserved for snapshot/public-read compatibility.
    /// True iff <see cref="Status"/> == <see cref="PhotoStatus.Public"/>.
    /// </summary>
    public bool IsPublic => Status == PhotoStatus.Public;

    private ListingImage() { }

    public static ListingImage Create(Guid listingId, string filePath, int displayOrder,
        string? thumbnailPath = null, string? altEn = null, string? altEs = null)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new DomainException("File path cannot be empty.");

        return new ListingImage
        {
            ListingId = listingId,
            FilePath = filePath,
            ThumbnailPath = thumbnailPath,
            AltEn = altEn,
            AltEs = altEs,
            DisplayOrder = displayOrder,
            Status = PhotoStatus.Pending
        };
    }

    /// <summary>
    /// Provider edits the alt text / display order. Does NOT change moderation status —
    /// re-ordering is metadata; image bytes are unchanged.
    /// </summary>
    public void UpdateMetadata(string? altEn, string? altEs, int displayOrder)
    {
        AltEn = altEn;
        AltEs = altEs;
        DisplayOrder = displayOrder;
    }

    /// <summary>
    /// GM moderation transition. Only path to mark a photo Public/Hidden/Rejected.
    /// </summary>
    public void Moderate(PhotoStatus newStatus, Guid adminId, string? gmFeedback = null)
    {
        if (newStatus == PhotoStatus.Pending)
            throw new DomainException("Cannot moderate back to Pending.");

        Status = newStatus;
        ModeratedAt = DateTimeOffset.UtcNow;
        ModeratedByAdminId = adminId;
        GmFeedback = gmFeedback;
    }

    // Legacy helpers used by seed/snapshot code — kept as thin shims over Moderate.
    public void MakePublic()
    {
        Status = PhotoStatus.Public;
        ModeratedAt ??= DateTimeOffset.UtcNow;
    }

    public void MakePrivate()
    {
        Status = PhotoStatus.Hidden;
        ModeratedAt ??= DateTimeOffset.UtcNow;
    }
}
