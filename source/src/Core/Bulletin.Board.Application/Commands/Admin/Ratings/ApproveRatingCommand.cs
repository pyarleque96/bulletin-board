using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Commands.Admin.Ratings;

public record ApproveRatingCommand(Guid RatingId) : IRequest;

public sealed class ApproveRatingCommandHandler(
    IRepository<Rating> ratingRepository,
    IListingRepository listingRepository,
    ICurrentUserService currentUserService,
    ILogger<ApproveRatingCommandHandler> logger)
    : IRequestHandler<ApproveRatingCommand>
{
    public async Task Handle(ApproveRatingCommand request, CancellationToken ct)
    {
        var rating = await ratingRepository.GetByIdAsync(request.RatingId, ct)
            ?? throw new InvalidOperationException($"Rating '{request.RatingId}' not found.");

        var adminId = currentUserService.UserId ?? throw new UnauthorizedAccessException();

        rating.Approve(adminId);
        ratingRepository.Update(rating);
        await ratingRepository.SaveChangesAsync(ct);

        logger.LogInformation("Rating {RatingId} approved by {AdminId}", request.RatingId, adminId);

        var approvedRatings = await ratingRepository.FindAsync(
            r => r.ListingId == rating.ListingId && r.Status == RatingStatus.Approved, ct);

        if (approvedRatings.Count > 0)
        {
            var avg = approvedRatings.Average(r => (decimal)r.Stars);
            var listing = await listingRepository.GetByIdAsync(rating.ListingId, ct);
            if (listing is not null)
            {
                listing.UpdateAvgRating(Math.Round(avg, 2));
                listingRepository.Update(listing);
                await listingRepository.SaveChangesAsync(ct);

                logger.LogInformation("Listing {ListingId} AvgRating updated to {Avg}", rating.ListingId, avg);
            }
        }
    }
}

public sealed class ApproveRatingCommandValidator : AbstractValidator<ApproveRatingCommand>
{
    public ApproveRatingCommandValidator()
    {
        RuleFor(x => x.RatingId).NotEmpty();
    }
}
