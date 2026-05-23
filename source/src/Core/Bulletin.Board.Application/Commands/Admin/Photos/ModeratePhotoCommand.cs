using Bulletin.Board.Application.Snapshots;
using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Exceptions;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Commands.Admin.Photos;

/// <summary>
/// Admin (GM) moderation transition for a photo. Regla #4: solo el GM puede pasar fotos
/// a Public/Hidden/Rejected.
/// </summary>
public record ModeratePhotoCommand(
    Guid PhotoId,
    string Status,
    string? GmFeedback) : IRequest;

public sealed class ModeratePhotoCommandValidator : AbstractValidator<ModeratePhotoCommand>
{
    public ModeratePhotoCommandValidator()
    {
        RuleFor(x => x.PhotoId).NotEmpty();
        RuleFor(x => x.Status)
            .Must(s => s is "Public" or "Hidden" or "Rejected")
            .WithMessage("Status must be Public, Hidden, or Rejected.");
        RuleFor(x => x.GmFeedback).MaximumLength(2000);
    }
}

public sealed class ModeratePhotoCommandHandler(
    IRepository<ListingImage> images,
    IListingSnapshotRefresher snapshotRefresher,
    ICurrentUserService currentUser,
    ILogger<ModeratePhotoCommandHandler> logger)
    : IRequestHandler<ModeratePhotoCommand>
{
    public async Task Handle(ModeratePhotoCommand request, CancellationToken ct)
    {
        var adminId = currentUser.UserId ?? throw new UnauthorizedAccessException();

        if (!Enum.TryParse<PhotoStatus>(request.Status, ignoreCase: false, out var newStatus))
            throw new DomainException("Invalid photo status.");

        var photo = (await images.FindAsync(i => i.Id == request.PhotoId, ct)).FirstOrDefault()
            ?? throw new InvalidOperationException("Photo not found.");

        photo.Moderate(newStatus, adminId, request.GmFeedback);
        images.Update(photo);
        await images.SaveChangesAsync(ct);

        logger.LogInformation("Photo {PhotoId} moderated to {Status} by {AdminId}",
            photo.Id, newStatus, adminId);

        // Refrescar snapshot del listing si está Approved — el público debe ver el cambio.
        await snapshotRefresher.RefreshIfApprovedAsync(photo.ListingId, ct);
    }
}
