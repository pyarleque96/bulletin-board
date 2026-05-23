using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using MediatR;

namespace Bulletin.Board.Application.Queries.Listings;

public sealed class GetMyListingsQueryHandler(
    IListingRepository listings,
    ICurrentUserService currentUser)
    : IRequestHandler<GetMyListingsQuery, PagedResult<MyListingDto>>
{
    public async Task<PagedResult<MyListingDto>> Handle(
        GetMyListingsQuery request, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new UnauthorizedAccessException("User must be authenticated.");

        var paged = new PagedRequest(request.Page, request.PageSize);

        var (entities, total) = await listings.GetByProviderUserIdPagedAsync(
            userId, paged.Skip, paged.Take, ct);

        var items = entities
            .Select(l => new MyListingDto(
                Id: l.Id,
                Slug: l.Slug,
                TitleEn: l.TitleEn,
                TitleEs: l.TitleEs,
                CategorySlug: l.Category?.Slug ?? string.Empty,
                Status: l.Status.ToString(),
                Tier: l.ProviderTier.ToString(),
                HasPublishedSnapshot: !string.IsNullOrEmpty(l.PublishedSnapshot),
                IsPausedByOwner: l.IsPausedByOwner,
                ApprovedAt: l.PublishedAt,
                UpdatedAt: l.UpdatedAt,
                DescriptionEn: l.DescriptionEn,
                DescriptionEs: l.DescriptionEs,
                Price: l.Price,
                Location: l.Location,
                WhatsAppNumber: l.WhatsAppNumber))
            .ToList();

        return new PagedResult<MyListingDto>(items, paged.Page, paged.Take, total);
    }
}
