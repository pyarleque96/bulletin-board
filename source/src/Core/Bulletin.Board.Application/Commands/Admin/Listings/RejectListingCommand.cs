using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Commands.Admin.Listings;

public record RejectListingCommand(Guid ListingId, string Reason) : IRequest;

public sealed class RejectListingCommandHandler(
    IListingRepository listingRepository,
    ICurrentUserService currentUserService,
    ILogger<RejectListingCommandHandler> logger)
    : IRequestHandler<RejectListingCommand>
{
    public async Task Handle(RejectListingCommand request, CancellationToken ct)
    {
        var listing = await listingRepository.GetByIdAsync(request.ListingId, ct)
            ?? throw new InvalidOperationException($"Listing '{request.ListingId}' not found.");

        var adminId = currentUserService.UserId ?? throw new UnauthorizedAccessException();

        listing.Reject(request.Reason);
        listingRepository.Update(listing);
        await listingRepository.SaveChangesAsync(ct);

        logger.LogInformation("Listing {ListingId} rejected by {AdminId}", request.ListingId, adminId);
    }
}

public sealed class RejectListingCommandValidator : AbstractValidator<RejectListingCommand>
{
    public RejectListingCommandValidator()
    {
        RuleFor(x => x.ListingId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
    }
}
