using Bulletin.Board.Application.Commands.Listings;
using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using MediatR;

namespace Bulletin.Board.Application.Commands.Manage.Photos;

public record DeletePhotoCommand(Guid ListingId, Guid PhotoId) : IRequest;

public sealed class DeletePhotoCommandHandler(
    IListingRepository listings,
    IRepository<ListingImage> images,
    ICurrentUserService currentUser)
    : IRequestHandler<DeletePhotoCommand>
{
    public async Task Handle(DeletePhotoCommand request, CancellationToken ct)
    {
        await ListingOwnerAuthorization.AuthorizeAsync(listings, currentUser, request.ListingId, ct);

        var photo = (await images.FindAsync(
            i => i.Id == request.PhotoId && i.ListingId == request.ListingId, ct))
            .FirstOrDefault()
            ?? throw new InvalidOperationException("Photo not found.");

        images.Remove(photo);
        await images.SaveChangesAsync(ct);
    }
}
