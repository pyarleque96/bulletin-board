using Bulletin.Board.Application.Commands.Listings;
using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using MediatR;

namespace Bulletin.Board.Application.Queries.Manage;

public record GetReviewSummaryQuery(Guid ListingId) : IRequest<RatingSummaryDto>;

public sealed class GetReviewSummaryQueryHandler(
    IListingRepository listings,
    IManageReviewsReader reader,
    ICurrentUserService currentUser)
    : IRequestHandler<GetReviewSummaryQuery, RatingSummaryDto>
{
    public async Task<RatingSummaryDto> Handle(GetReviewSummaryQuery request, CancellationToken ct)
    {
        await ListingOwnerAuthorization.AuthorizeAsync(listings, currentUser, request.ListingId, ct);
        return await reader.GetSummaryAsync(request.ListingId, ct);
    }
}
