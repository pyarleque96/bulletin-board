using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Exceptions;
using FluentAssertions;

namespace Bulletin.Board.Domain.Tests.Entities;

/// <summary>
/// Tests for regla #7: ratings y provider replies pasan por moderación del GM.
/// </summary>
public class ReviewReplyTests
{
    [Fact]
    public void Create_initializes_reply_to_pending_gm()
    {
        var reply = ReviewReply.Create(Guid.NewGuid(), "Thanks for the feedback!");

        reply.Status.Should().Be(ReviewReplyStatus.PendingGm);
        reply.ModeratedAt.Should().BeNull();
        reply.ModeratedByAdminId.Should().BeNull();
    }

    [Fact]
    public void Create_throws_when_text_empty()
    {
        var act = () => ReviewReply.Create(Guid.NewGuid(), string.Empty);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_throws_when_text_too_long()
    {
        var act = () => ReviewReply.Create(Guid.NewGuid(), new string('a', 2001));
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Approve_publishes_reply_and_records_admin()
    {
        var reply = ReviewReply.Create(Guid.NewGuid(), "Thanks!");
        var adminId = Guid.NewGuid();

        reply.Approve(adminId);

        reply.Status.Should().Be(ReviewReplyStatus.Published);
        reply.ModeratedByAdminId.Should().Be(adminId);
        reply.ModeratedAt.Should().NotBeNull();
    }

    [Fact]
    public void Reject_records_gm_feedback()
    {
        var reply = ReviewReply.Create(Guid.NewGuid(), "Inappropriate response");

        reply.Reject(Guid.NewGuid(), "Tone not aligned with platform values");

        reply.Status.Should().Be(ReviewReplyStatus.Rejected);
        reply.GmFeedback.Should().Be("Tone not aligned with platform values");
    }

    [Fact]
    public void Rating_AttachReply_creates_pending_reply()
    {
        var rating = Rating.Create(
            contactId: Guid.NewGuid(),
            listingId: Guid.NewGuid(),
            providerId: Guid.NewGuid(),
            authorUserId: Guid.NewGuid(),
            stars: 5,
            comment: "Great");

        var reply = rating.AttachReply("Thank you!");

        rating.Reply.Should().NotBeNull();
        reply.Status.Should().Be(ReviewReplyStatus.PendingGm);
    }

    [Fact]
    public void Rating_AttachReply_throws_if_reply_already_exists()
    {
        var rating = Rating.Create(
            contactId: Guid.NewGuid(),
            listingId: Guid.NewGuid(),
            providerId: Guid.NewGuid(),
            authorUserId: Guid.NewGuid(),
            stars: 5,
            comment: "Great");
        rating.AttachReply("First reply");

        var act = () => rating.AttachReply("Second reply");

        act.Should().Throw<DomainException>();
    }
}
