using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Commands.Admin.Ratings;

public record RejectRatingCommand(Guid RatingId) : IRequest;

public sealed class RejectRatingCommandHandler(
    IRepository<Rating> ratingRepository,
    ICurrentUserService currentUserService,
    ILogger<RejectRatingCommandHandler> logger)
    : IRequestHandler<RejectRatingCommand>
{
    public async Task Handle(RejectRatingCommand request, CancellationToken ct)
    {
        var rating = await ratingRepository.GetByIdAsync(request.RatingId, ct)
            ?? throw new InvalidOperationException($"Rating '{request.RatingId}' not found.");

        var adminId = currentUserService.UserId ?? throw new UnauthorizedAccessException();

        rating.Reject();
        ratingRepository.Update(rating);
        await ratingRepository.SaveChangesAsync(ct);

        logger.LogInformation("Rating {RatingId} rejected by {AdminId}", request.RatingId, adminId);
    }
}

public sealed class RejectRatingCommandValidator : AbstractValidator<RejectRatingCommand>
{
    public RejectRatingCommandValidator()
    {
        RuleFor(x => x.RatingId).NotEmpty();
    }
}
