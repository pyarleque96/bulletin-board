using Bulletin.Board.Application.Commands.Listings;
using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using MediatR;

namespace Bulletin.Board.Application.Queries.Manage;

public record GetListingPhotosQuery(Guid ListingId) : IRequest<IReadOnlyList<PhotoDto>>;

public sealed class GetListingPhotosQueryHandler(
    IListingRepository listings,
    IRepository<ListingImage> images,
    ICurrentUserService currentUser)
    : IRequestHandler<GetListingPhotosQuery, IReadOnlyList<PhotoDto>>
{
    public async Task<IReadOnlyList<PhotoDto>> Handle(GetListingPhotosQuery request, CancellationToken ct)
    {
        await ListingOwnerAuthorization.AuthorizeAsync(listings, currentUser, request.ListingId, ct);

        var photos = await images.FindAsync(i => i.ListingId == request.ListingId, ct);

        return photos
            .OrderBy(i => i.DisplayOrder)
            .ThenBy(i => i.UploadedAt)
            .Select(i => new PhotoDto(
                Id: i.Id,
                Url: i.FilePath,
                ThumbnailUrl: i.ThumbnailPath ?? i.FilePath,
                AltEn: i.AltEn,
                AltEs: i.AltEs,
                Status: i.Status.ToString(),
                Position: i.DisplayOrder,
                GmFeedback: i.GmFeedback,
                UploadedAt: i.UploadedAt))
            .ToList();
    }
}
