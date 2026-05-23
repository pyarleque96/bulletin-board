using Bulletin.Board.Application.Snapshots;
using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Commands.Admin.Images;

public record SetImageVisibilityCommand(Guid ListingId, Guid ImageId, bool IsPublic) : IRequest;

public sealed class SetImageVisibilityCommandHandler(
    IRepository<ListingImage> imageRepository,
    IListingSnapshotRefresher snapshotRefresher,
    ICurrentUserService currentUserService,
    ILogger<SetImageVisibilityCommandHandler> logger)
    : IRequestHandler<SetImageVisibilityCommand>
{
    public async Task Handle(SetImageVisibilityCommand request, CancellationToken ct)
    {
        var images = await imageRepository.FindAsync(
            i => i.Id == request.ImageId && i.ListingId == request.ListingId, ct);

        var image = images.FirstOrDefault()
            ?? throw new InvalidOperationException($"Image '{request.ImageId}' not found for listing '{request.ListingId}'.");

        var adminId = currentUserService.UserId ?? throw new UnauthorizedAccessException();

        if (request.IsPublic)
            image.MakePublic();
        else
            image.MakePrivate();

        imageRepository.Update(image);
        await imageRepository.SaveChangesAsync(ct);

        logger.LogInformation("Image {ImageId} visibility set to {IsPublic} by {AdminId}",
            request.ImageId, request.IsPublic, adminId);

        // Reflect the visibility change in the published snapshot if listing is Approved.
        await snapshotRefresher.RefreshIfApprovedAsync(request.ListingId, ct);
    }
}

public sealed class SetImageVisibilityCommandValidator : AbstractValidator<SetImageVisibilityCommand>
{
    public SetImageVisibilityCommandValidator()
    {
        RuleFor(x => x.ListingId).NotEmpty();
        RuleFor(x => x.ImageId).NotEmpty();
    }
}
