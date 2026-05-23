using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Events;

namespace Bulletin.Board.Domain.Entities;

public class Rating
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ContactId { get; private set; }
    public Guid ListingId { get; private set; }
    public Guid ProviderId { get; private set; }
    public Guid AuthorUserId { get; private set; }
    public short Stars { get; private set; }
    public string? Comment { get; private set; }
    public RatingStatus Status { get; private set; } = RatingStatus.Pending;
    public string? GmFeedback { get; private set; }
    public DateTimeOffset? ApprovedAt { get; private set; }
    public Guid? ApprovedBy { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public Contact Contact { get; private set; } = null!;
    public Listing Listing { get; private set; } = null!;
    public Provider Provider { get; private set; } = null!;
    public ReviewReply? Reply { get; private set; }

    private readonly List<object> _domainEvents = [];
    public IReadOnlyList<object> DomainEvents => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();

    private Rating() { }

    public static Rating Create(Guid contactId, Guid listingId, Guid providerId, Guid authorUserId, short stars, string? comment)
    {
        if (stars is < 1 or > 5)
            throw new ArgumentException("Stars must be between 1 and 5.");

        return new Rating
        {
            ContactId = contactId,
            ListingId = listingId,
            ProviderId = providerId,
            AuthorUserId = authorUserId,
            Stars = stars,
            Comment = comment
        };
    }

    public void Approve(Guid approverId)
    {
        Status = RatingStatus.Approved;
        ApprovedAt = DateTimeOffset.UtcNow;
        ApprovedBy = approverId;
        _domainEvents.Add(new RatingApprovedEvent(Id, ListingId, ProviderId, Stars));
    }

    public void Reject(string? gmFeedback = null)
    {
        Status = RatingStatus.Rejected;
        GmFeedback = gmFeedback;
    }

    /// <summary>
    /// Provider posts a reply to this review. Per regla #7, the reply also enters GM moderation.
    /// Throws if a reply already exists — providers may not re-reply; they must wait moderation.
    /// </summary>
    public ReviewReply AttachReply(string text)
    {
        if (Reply is not null)
            throw new Bulletin.Board.Domain.Exceptions.DomainException("This review already has a reply pending moderation or published.");
        Reply = ReviewReply.Create(Id, text);
        return Reply;
    }
}
