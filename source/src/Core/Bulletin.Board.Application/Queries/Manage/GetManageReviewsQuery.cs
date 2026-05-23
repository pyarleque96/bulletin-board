using Bulletin.Board.Application.Commands.Listings;
using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using MediatR;

namespace Bulletin.Board.Application.Queries.Manage;

/// <summary>
/// Provider-side reviews query. Frontend espera statuses: "Published" | "PendingGm" | "Rejected".
/// El mapeo desde el dominio: Approved → Published, Pending → PendingGm.
/// </summary>
public record GetManageReviewsQuery(Guid ListingId, string? Status) : IRequest<IReadOnlyList<ManageReviewDto>>;

public sealed class GetManageReviewsQueryHandler(
    IListingRepository listings,
    IManageReviewsReader reader,
    ICurrentUserService currentUser)
    : IRequestHandler<GetManageReviewsQuery, IReadOnlyList<ManageReviewDto>>
{
    public async Task<IReadOnlyList<ManageReviewDto>> Handle(GetManageReviewsQuery request, CancellationToken ct)
    {
        await ListingOwnerAuthorization.AuthorizeAsync(listings, currentUser, request.ListingId, ct);

        RatingStatus? filter = request.Status switch
        {
            "Published" => RatingStatus.Approved,
            "PendingGm" => RatingStatus.Pending,
            "Rejected" => RatingStatus.Rejected,
            "all" or null or "" => null,
            _ => null
        };

        return await reader.GetReviewsAsync(request.ListingId, filter, ct);
    }
}

/// <summary>
/// Read-side facade for manage reviews. Implementación EF en Infrastructure carga el Reply
/// en una sola query para evitar N+1.
/// </summary>
public interface IManageReviewsReader
{
    Task<IReadOnlyList<ManageReviewDto>> GetReviewsAsync(
        Guid listingId, RatingStatus? statusFilter, CancellationToken ct);

    Task<RatingSummaryDto> GetSummaryAsync(Guid listingId, CancellationToken ct);
}
