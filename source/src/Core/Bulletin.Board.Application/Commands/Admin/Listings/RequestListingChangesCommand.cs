using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Commands.Admin.Listings;

public record RequestListingChangesCommand(Guid ListingId, string Feedback) : IRequest;

public sealed class RequestListingChangesCommandHandler(
    IListingRepository listingRepository,
    ICurrentUserService currentUserService,
    ILogger<RequestListingChangesCommandHandler> logger)
    : IRequestHandler<RequestListingChangesCommand>
{
    public async Task Handle(RequestListingChangesCommand request, CancellationToken ct)
    {
        var listing = await listingRepository.GetByIdAsync(request.ListingId, ct)
            ?? throw new InvalidOperationException($"Listing '{request.ListingId}' not found.");

        var adminId = currentUserService.UserId ?? throw new UnauthorizedAccessException();

        listing.RequestChanges(request.Feedback);
        listingRepository.Update(listing);
        await listingRepository.SaveChangesAsync(ct);

        logger.LogInformation("Listing {ListingId} needs changes, requested by {AdminId}", request.ListingId, adminId);
    }
}

public sealed class RequestListingChangesCommandValidator : AbstractValidator<RequestListingChangesCommand>
{
    public RequestListingChangesCommandValidator()
    {
        RuleFor(x => x.ListingId).NotEmpty();
        RuleFor(x => x.Feedback).NotEmpty().MaximumLength(1000);
    }
}
