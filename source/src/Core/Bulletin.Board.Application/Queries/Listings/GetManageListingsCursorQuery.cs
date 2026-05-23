using Bulletin.Board.Application.Common.Models;
using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Domain.Enums;
using MediatR;

namespace Bulletin.Board.Application.Queries.Listings;

/// <summary>
/// Cursor-paginated query for the /manage index (infinity scroll).
/// Page+pageSize compatibility is provided by the existing <see cref="GetMyListingsQuery"/>
/// for one sprint — this query supersedes it.
/// </summary>
public record GetManageListingsCursorQuery(
    string? Cursor,
    int PageSize,
    ListingStatus? Status,
    string? CategorySlug,
    string? Search,
    bool IncludeTotal)
    : IRequest<CursorPagedResultDto<MyListingDto>>;
