using Bulletin.Board.Application.DTOs.Admin;
using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Queries.Admin.Images;

public record GetListingImagesQuery(Guid ListingId) : IRequest<IReadOnlyList<AdminImageDto>>;

public sealed class GetListingImagesQueryHandler(
    IRepository<ListingImage> imageRepository,
    ILogger<GetListingImagesQueryHandler> logger)
    : IRequestHandler<GetListingImagesQuery, IReadOnlyList<AdminImageDto>>
{
    public async Task<IReadOnlyList<AdminImageDto>> Handle(GetListingImagesQuery request, CancellationToken ct)
    {
        logger.LogInformation("Admin fetching all images for listing {ListingId}", request.ListingId);

        var images = await imageRepository.FindAsync(i => i.ListingId == request.ListingId, ct);

        return images
            .OrderBy(i => i.DisplayOrder)
            .Select(i => new AdminImageDto(
                Id: i.Id,
                ListingId: i.ListingId,
                FilePath: i.FilePath,
                IsPublic: i.IsPublic,
                DisplayOrder: i.DisplayOrder,
                UploadedAt: i.UploadedAt))
            .ToList();
    }
}
