using Bulletin.Board.Application.Commands.Listings;
using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using FluentValidation;
using MediatR;

namespace Bulletin.Board.Application.Commands.Manage.Reviews;

/// <summary>
/// Provider submits a reply to a review. Per regla #7 the reply enters GM moderation
/// (Status=PendingGm) and is only visible to the public after Admin approval.
/// </summary>
public record SubmitReviewReplyCommand(Guid ListingId, Guid ReviewId, string Text)
    : IRequest<ReviewReplyDto>;

public sealed class SubmitReviewReplyCommandValidator : AbstractValidator<SubmitReviewReplyCommand>
{
    public SubmitReviewReplyCommandValidator()
    {
        RuleFor(x => x.ListingId).NotEmpty();
        RuleFor(x => x.ReviewId).NotEmpty();
        RuleFor(x => x.Text).NotEmpty().MaximumLength(2000);
    }
}

public sealed class SubmitReviewReplyCommandHandler(
    IListingRepository listings,
    IRepository<Rating> ratings,
    ICurrentUserService currentUser)
    : IRequestHandler<SubmitReviewReplyCommand, ReviewReplyDto>
{
    public async Task<ReviewReplyDto> Handle(SubmitReviewReplyCommand request, CancellationToken ct)
    {
        await ListingOwnerAuthorization.AuthorizeAsync(listings, currentUser, request.ListingId, ct);

        var rating = (await ratings.FindAsync(
            r => r.Id == request.ReviewId && r.ListingId == request.ListingId, ct))
            .FirstOrDefault()
            ?? throw new InvalidOperationException("Review not found.");

        var reply = rating.AttachReply(request.Text);
        ratings.Update(rating);
        await ratings.SaveChangesAsync(ct);

        return new ReviewReplyDto(reply.Id, reply.Text, reply.Status.ToString(), reply.CreatedAt);
    }
}
