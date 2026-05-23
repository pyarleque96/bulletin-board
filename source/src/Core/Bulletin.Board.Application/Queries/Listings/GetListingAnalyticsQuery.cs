using Bulletin.Board.Application.DTOs;
using MediatR;

namespace Bulletin.Board.Application.Queries.Listings;

/// <summary>Owner-only. Returns KPI metrics for a listing's dashboard.</summary>
public record GetListingAnalyticsQuery(Guid ListingId) : IRequest<ListingAnalyticsDto>;
