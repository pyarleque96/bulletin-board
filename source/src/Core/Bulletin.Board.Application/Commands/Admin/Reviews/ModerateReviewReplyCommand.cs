using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Commands.Admin.Reviews;

/// <summary>
/// Admin (GM) moderation of a provider reply to a review. Regla #7.
/// </summary>
public record ModerateReviewReplyCommand(Guid ReplyId, bool Approve, string? GmFeedback) : IRequest;

public sealed class ModerateReviewReplyCommandValidator : AbstractValidator<ModerateReviewReplyCommand>
{
    public ModerateReviewReplyCommandValidator()
    {
        RuleFor(x => x.ReplyId).NotEmpty();
        RuleFor(x => x.GmFeedback).MaximumLength(2000);
    }
}

public sealed class ModerateReviewReplyCommandHandler(
    IRepository<ReviewReply> replies,
    ICurrentUserService currentUser,
    ILogger<ModerateReviewReplyCommandHandler> logger)
    : IRequestHandler<ModerateReviewReplyCommand>
{
    public async Task Handle(ModerateReviewReplyCommand request, CancellationToken ct)
    {
        var adminId = currentUser.UserId ?? throw new UnauthorizedAccessException();

        var reply = (await replies.FindAsync(r => r.Id == request.ReplyId, ct)).FirstOrDefault()
            ?? throw new InvalidOperationException("Reply not found.");

        if (request.Approve) reply.Approve(adminId);
        else reply.Reject(adminId, request.GmFeedback);

        replies.Update(reply);
        await replies.SaveChangesAsync(ct);

        logger.LogInformation("Review reply {ReplyId} {Action} by {AdminId}",
            reply.Id, request.Approve ? "approved" : "rejected", adminId);
    }
}
