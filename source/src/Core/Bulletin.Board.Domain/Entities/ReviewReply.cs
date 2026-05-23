using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Exceptions;

namespace Bulletin.Board.Domain.Entities;

/// <summary>
/// Provider's reply to a <see cref="Rating"/>. Regla #7: el reply pasa por moderación GM
/// antes de ser visible al público. Estados: PendingGm → Published | Rejected.
/// </summary>
public class ReviewReply
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ReviewId { get; private set; }
    public string Text { get; private set; } = string.Empty;
    public ReviewReplyStatus Status { get; private set; } = ReviewReplyStatus.PendingGm;
    public string? GmFeedback { get; private set; }
    public DateTimeOffset? ModeratedAt { get; private set; }
    public Guid? ModeratedByAdminId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public Rating Review { get; private set; } = null!;

    private ReviewReply() { }

    public static ReviewReply Create(Guid reviewId, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new DomainException("Reply text cannot be empty.");
        if (text.Length > 2000)
            throw new DomainException("Reply text exceeds 2000 characters.");

        return new ReviewReply
        {
            ReviewId = reviewId,
            Text = text,
            Status = ReviewReplyStatus.PendingGm
        };
    }

    /// <summary>
    /// GM approves the reply, making it visible to the public.
    /// </summary>
    public void Approve(Guid adminId)
    {
        Status = ReviewReplyStatus.Published;
        ModeratedAt = DateTimeOffset.UtcNow;
        ModeratedByAdminId = adminId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Reject(Guid adminId, string? gmFeedback = null)
    {
        Status = ReviewReplyStatus.Rejected;
        ModeratedAt = DateTimeOffset.UtcNow;
        ModeratedByAdminId = adminId;
        GmFeedback = gmFeedback;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
