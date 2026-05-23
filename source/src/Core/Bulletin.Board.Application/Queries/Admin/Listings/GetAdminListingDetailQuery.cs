using Bulletin.Board.Application.DTOs.Admin;
using Bulletin.Board.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Queries.Admin.Listings;

public record GetAdminListingDetailQuery(Guid ListingId) : IRequest<AdminListingDetailDto?>;

public sealed class GetAdminListingDetailQueryHandler(
    IListingRepository listingRepository,
    ILogger<GetAdminListingDetailQueryHandler> logger)
    : IRequestHandler<GetAdminListingDetailQuery, AdminListingDetailDto?>
{
    public async Task<AdminListingDetailDto?> Handle(GetAdminListingDetailQuery request, CancellationToken ct)
    {
        logger.LogInformation("Admin fetching listing detail for {ListingId}", request.ListingId);

        var listing = await listingRepository.GetAdminDetailAsync(request.ListingId, ct);
        if (listing is null)
            return null;

        var allImages = listing.Images
            .OrderBy(i => i.DisplayOrder)
            .Select(i => new AdminImageDto(
                Id: i.Id,
                ListingId: i.ListingId,
                FilePath: i.FilePath,
                IsPublic: i.IsPublic,
                DisplayOrder: i.DisplayOrder,
                UploadedAt: i.UploadedAt))
            .ToList();

        return new AdminListingDetailDto(
            Id: listing.Id,
            TitleEn: listing.TitleEn,
            TitleEs: listing.TitleEs,
            DescriptionEn: listing.DescriptionEn,
            DescriptionEs: listing.DescriptionEs,
            Price: listing.Price,
            Location: listing.Location,
            WhatsAppNumber: listing.WhatsAppNumber,
            Status: listing.Status.ToString(),
            RejectionReason: listing.RejectionReason,
            ProviderTier: listing.ProviderTier.ToString(),
            AvgRating: listing.AvgRating,
            IsDeleted: listing.IsDeleted,
            CreatedAt: listing.CreatedAt,
            PublishedAt: listing.PublishedAt,
            ProviderId: listing.ProviderId,
            ProviderName: listing.Provider?.WhatsAppNumber ?? listing.ProviderId.ToString(),
            CategoryId: listing.CategoryId,
            CategoryName: listing.Category?.NameEn ?? listing.CategoryId.ToString(),
            Images: allImages);
    }
}
