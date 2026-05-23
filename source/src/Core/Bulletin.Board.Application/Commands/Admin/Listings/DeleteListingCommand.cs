using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Commands.Admin.Listings;

public record DeleteListingCommand(Guid ListingId) : IRequest;

public sealed class DeleteListingCommandHandler(
    IListingRepository listingRepository,
    ICurrentUserService currentUserService,
    ILogger<DeleteListingCommandHandler> logger)
    : IRequestHandler<DeleteListingCommand>
{
    public async Task Handle(DeleteListingCommand request, CancellationToken ct)
    {
        var listing = await listingRepository.GetByIdAsync(request.ListingId, ct)
            ?? throw new InvalidOperationException($"Listing '{request.ListingId}' not found.");

        var adminId = currentUserService.UserId ?? throw new UnauthorizedAccessException();

        listing.SoftDelete();
        listingRepository.Update(listing);
        await listingRepository.SaveChangesAsync(ct);

        logger.LogInformation("Listing {ListingId} soft-deleted by {AdminId}", request.ListingId, adminId);
    }
}

public sealed class DeleteListingCommandValidator : AbstractValidator<DeleteListingCommand>
{
    public DeleteListingCommandValidator()
    {
        RuleFor(x => x.ListingId).NotEmpty();
    }
}
