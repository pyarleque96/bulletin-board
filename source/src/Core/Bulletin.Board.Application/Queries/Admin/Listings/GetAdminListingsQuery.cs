using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Application.DTOs.Admin;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Queries.Admin.Listings;

public record GetAdminListingsQuery(
    string? Status,
    string? Tier,
    Guid? CategoryId,
    Guid? ProviderId,
    int Page,
    int PageSize) : IRequest<PagedResult<AdminListingDto>>;

public sealed class GetAdminListingsQueryHandler(
    IListingRepository listingRepository,
    ILogger<GetAdminListingsQueryHandler> logger)
    : IRequestHandler<GetAdminListingsQuery, PagedResult<AdminListingDto>>
{
    public async Task<PagedResult<AdminListingDto>> Handle(GetAdminListingsQuery request, CancellationToken ct)
    {
        logger.LogInformation(
            "Admin fetching listings status={Status} tier={Tier} categoryId={CategoryId} providerId={ProviderId}",
            request.Status, request.Tier, request.CategoryId, request.ProviderId);

        ListingStatus? statusFilter = Enum.TryParse<ListingStatus>(request.Status, ignoreCase: true, out var s)
            ? s : null;

        ProviderTier? tierFilter = Enum.TryParse<ProviderTier>(request.Tier, ignoreCase: true, out var t)
            ? t : null;

        var (items, total) = await listingRepository.GetAdminListingsPagedAsync(
            statusFilter,
            tierFilter,
            request.CategoryId,
            request.ProviderId,
            request.Page,
            request.PageSize,
            ct);

        var userIds = items
            .Select(l => l.Provider?.UserId)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct();

        var nameMap = await listingRepository.GetProviderDisplayNamesByUserIdsAsync(userIds, ct);

        var now = DateTimeOffset.UtcNow;
        var paged = items.Select(l => new AdminListingDto(
            Id: l.Id,
            TitleEn: l.TitleEn,
            TitleEs: l.TitleEs,
            ProviderName: l.Provider is not null && nameMap.TryGetValue(l.Provider.UserId, out var name)
                ? name
                : l.Provider?.WhatsAppNumber ?? l.ProviderId.ToString(),
            ProviderPhone: l.Provider?.WhatsAppNumber,
            ProviderTier: l.ProviderTier.ToString(),
            CategoryName: l.Category?.NameEn ?? l.CategoryId.ToString(),
            Status: l.Status.ToString(),
            CreatedAt: l.CreatedAt,
            UpdatedAt: l.UpdatedAt,
            AvgRating: l.AvgRating,
            PhotoCount: l.Images.Count,
            IsVipNow: l.IsVipNow(now),
            VipUntil: l.VipUntil,
            VipIsIndefinite: l.VipIsIndefinite))
            .ToList();

        return new PagedResult<AdminListingDto>(paged, total, request.Page, request.PageSize);
    }
}

public sealed class GetAdminListingsQueryValidator : AbstractValidator<GetAdminListingsQuery>
{
    public GetAdminListingsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
