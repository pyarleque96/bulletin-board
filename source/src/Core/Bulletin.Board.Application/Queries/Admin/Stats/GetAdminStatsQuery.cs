using Bulletin.Board.Application.DTOs.Admin;
using Bulletin.Board.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Queries.Admin.Stats;

public record GetAdminStatsQuery : IRequest<AdminStatsDto>;

public sealed class GetAdminStatsQueryHandler(
    IAdminStatsRepository statsRepository,
    ILogger<GetAdminStatsQueryHandler> logger)
    : IRequestHandler<GetAdminStatsQuery, AdminStatsDto>
{
    public async Task<AdminStatsDto> Handle(GetAdminStatsQuery request, CancellationToken ct)
    {
        logger.LogInformation("Admin fetching dashboard stats");

        // EF Core DbContext is not thread-safe: all queries must be awaited sequentially.
        // Task.WhenAll over the same DbContext instance causes a concurrent-operation exception.
        var pendingListings   = await statsRepository.CountPendingListingsAsync(ct);
        var pendingReviews    = await statsRepository.CountPendingRatingsAsync(ct);
        var contactsToday     = await statsRepository.CountContactsTodayAsync(ct);
        var totalProviders    = await statsRepository.CountTotalProvidersAsync(ct);
        var vipProviders      = await statsRepository.CountVipProvidersAsync(ct);
        var verifiedProviders = await statsRepository.CountVerifiedProvidersAsync(ct);
        var regularProviders  = await statsRepository.CountRegularProvidersAsync(ct);
        var urgentListings    = await statsRepository.CountUrgentListingsAsync(ct);

        return new AdminStatsDto(
            PendingListings:   pendingListings,
            PendingReviews:    pendingReviews,
            ContactsToday:     contactsToday,
            TotalProviders:    totalProviders,
            VipProviders:      vipProviders,
            VerifiedProviders: verifiedProviders,
            RegularProviders:  regularProviders,
            UrgentListings:    urgentListings);
    }
}
