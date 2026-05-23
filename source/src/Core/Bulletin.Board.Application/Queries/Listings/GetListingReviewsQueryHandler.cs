using Bulletin.Board.Application.Commands.Listings;
using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Queries.Listings;

public sealed class GetListingReviewsQueryHandler(
    IListingRepository listings,
    IRepository<Domain.Entities.Rating> ratings,
    ICurrentUserService currentUser,
    ILogger<GetListingReviewsQueryHandler> logger)
    : IRequestHandler<GetListingReviewsQuery, PagedResult<ListingReviewDto>>
{
    public async Task<PagedResult<ListingReviewDto>> Handle(
        GetListingReviewsQuery request, CancellationToken ct)
    {
        await ListingOwnerAuthorization.AuthorizeAsync(listings, currentUser, request.ListingId, ct);

        // Regla #7: only ratings from users who actually contacted via the platform.
        // The Rating entity is linked to a Contact, so Approved ratings already satisfy this invariant.
        var allApproved = await ratings.FindAsync(
            r => r.ListingId == request.ListingId && r.Status == RatingStatus.Approved, ct);

        var sorted = allApproved
            .OrderByDescending(r => r.CreatedAt)
            .ToList();

        var totalCount = sorted.Count;

        var page = sorted
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(r => new ListingReviewDto(
                Id: r.Id,
                ReviewerName: "Anonymous",      // never expose real user names publicly
                Rating: r.Stars,
                Comment: r.Comment,
                CreatedAt: r.CreatedAt,
                IsVerified: true))              // contact-verified by definition
            .ToList();

        logger.LogInformation(
            "Reviews for listing {ListingId}: total={Total}, page={Page}.",
            request.ListingId, totalCount, request.Page);

        return new PagedResult<ListingReviewDto>(page, request.Page, request.PageSize, totalCount);
    }
}
