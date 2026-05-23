using Bulletin.Board.Application.Commands.Listings;
using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using FluentValidation;
using MediatR;

namespace Bulletin.Board.Application.Commands.Manage.Photos;

/// <summary>
/// Provider edits the alt text and/or display order of a photo it owns.
/// Re-ordering does NOT change moderation status (regla #4 — el bitmap es el mismo,
/// solo metadata cambia).
/// </summary>
public record UpdatePhotoMetadataCommand(
    Guid ListingId,
    Guid PhotoId,
    string? AltEn,
    string? AltEs,
    int Position) : IRequest<PhotoDto>;

public sealed class UpdatePhotoMetadataCommandValidator : AbstractValidator<UpdatePhotoMetadataCommand>
{
    public UpdatePhotoMetadataCommandValidator()
    {
        RuleFor(x => x.ListingId).NotEmpty();
        RuleFor(x => x.PhotoId).NotEmpty();
        RuleFor(x => x.Position).GreaterThanOrEqualTo(0);
        RuleFor(x => x.AltEn).MaximumLength(255);
        RuleFor(x => x.AltEs).MaximumLength(255);
    }
}

public sealed class UpdatePhotoMetadataCommandHandler(
    IListingRepository listings,
    IRepository<ListingImage> images,
    ICurrentUserService currentUser)
    : IRequestHandler<UpdatePhotoMetadataCommand, PhotoDto>
{
    public async Task<PhotoDto> Handle(UpdatePhotoMetadataCommand request, CancellationToken ct)
    {
        await ListingOwnerAuthorization.AuthorizeAsync(listings, currentUser, request.ListingId, ct);

        var photo = (await images.FindAsync(
            i => i.Id == request.PhotoId && i.ListingId == request.ListingId, ct))
            .FirstOrDefault()
            ?? throw new InvalidOperationException("Photo not found.");

        photo.UpdateMetadata(request.AltEn, request.AltEs, request.Position);
        images.Update(photo);
        await images.SaveChangesAsync(ct);

        return new PhotoDto(
            photo.Id, photo.FilePath, photo.ThumbnailPath ?? photo.FilePath,
            photo.AltEn, photo.AltEs, photo.Status.ToString(),
            photo.DisplayOrder, photo.GmFeedback, photo.UploadedAt);
    }
}
