using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Application.DTOs.Admin;
using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Queries.Admin.Providers;

public record GetAdminProvidersQuery(
    string? Tier,
    string? VerificationStatus,
    int Page,
    int PageSize,
    string? SortBy = null,
    string? SortDir = null)
    : IRequest<PagedResult<AdminProviderDto>>;

public sealed class GetAdminProvidersQueryHandler(
    IRepository<Provider> providerRepository,
    IRepository<Verification> verificationRepository,
    IUserService userService,
    ILogger<GetAdminProvidersQueryHandler> logger)
    : IRequestHandler<GetAdminProvidersQuery, PagedResult<AdminProviderDto>>
{
    public async Task<PagedResult<AdminProviderDto>> Handle(GetAdminProvidersQuery request, CancellationToken ct)
    {
        logger.LogInformation(
            "Admin fetching providers tier={Tier} verification={Verification} sortBy={SortBy} sortDir={SortDir}",
            request.Tier, request.VerificationStatus, request.SortBy, request.SortDir);

        var all = await providerRepository.GetAllAsync(ct);
        var filtered = all.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(request.Tier) &&
            Enum.TryParse<ProviderTier>(request.Tier, ignoreCase: true, out var tier))
            filtered = filtered.Where(p => p.Tier == tier);

        if (!string.IsNullOrWhiteSpace(request.VerificationStatus) &&
            Enum.TryParse<VerificationStatus>(request.VerificationStatus, ignoreCase: true, out var vs))
            filtered = filtered.Where(p => p.VerificationStatus == vs);

        var isAscending = string.Equals(request.SortDir, "asc", StringComparison.OrdinalIgnoreCase);

        var sorted = (request.SortBy?.ToLowerInvariant() switch
        {
            "hierarchy"  => isAscending
                                ? filtered.OrderBy(p => p.Hierarchy)
                                : filtered.OrderByDescending(p => p.Hierarchy),
            "tier"       => isAscending
                                ? filtered.OrderBy(p => (int)p.Tier)
                                : filtered.OrderByDescending(p => (int)p.Tier),
            "listings"   => isAscending
                                ? filtered.OrderBy(p => p.Listings.Count)
                                : filtered.OrderByDescending(p => p.Listings.Count),
            "createdat"  => isAscending
                                ? filtered.OrderBy(p => p.CreatedAt)
                                : filtered.OrderByDescending(p => p.CreatedAt),
            "updatedat"  => isAscending
                                ? filtered.OrderBy(p => p.UpdatedAt)
                                : filtered.OrderByDescending(p => p.UpdatedAt),
            // Default para manage: última modificación arriba (COALESCE semántico: UpdatedAt siempre tiene valor en Provider)
            _ => filtered.OrderByDescending(p => p.UpdatedAt)
        }).ToList();
        var total = sorted.Count;

        var now = DateTimeOffset.UtcNow;
        var pageItems = sorted
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var pageIds = pageItems.Select(p => p.Id).ToHashSet();
        var pageUserIds = pageItems.Select(p => p.UserId).ToHashSet();
        var verifications = await verificationRepository.FindAsync(v => pageIds.Contains(v.ProviderId), ct);
        var verifByProvider = verifications.ToDictionary(v => v.ProviderId);
        var names = await userService.GetNamesAsync(pageUserIds, ct);

        var paged = pageItems.Select(p =>
        {
            verifByProvider.TryGetValue(p.Id, out var v);
            names.TryGetValue(p.UserId, out var n);
            return new AdminProviderDto(
                Id: p.Id,
                UserId: p.UserId,
                FirstName: n.FirstName,
                LastName: n.LastName,
                Tier: p.Tier.ToString(),
                VerificationStatus: p.VerificationStatus.ToString(),
                WhatsAppNumber: p.WhatsAppNumber,
                Bio: p.Bio,
                Hierarchy: p.Hierarchy,
                CreatedAt: p.CreatedAt,
                UpdatedAt: p.UpdatedAt,
                ListingsCount: p.Listings.Count,
                IsVipNow: p.IsVipNow(now),
                VipUntil: p.VipUntil,
                VipIsIndefinite: p.VipIsIndefinite,
                PhoneVerified: v?.PhoneVerifiedAt.HasValue ?? false,
                IdVerified: v?.IdentityStatus == VerificationStatus.Approved,
                SocialVerified: !string.IsNullOrWhiteSpace(v?.SocialProfilesJson));
        }).ToList();

        return new PagedResult<AdminProviderDto>(paged, total, request.Page, request.PageSize);
    }
}

public sealed class GetAdminProvidersQueryValidator : AbstractValidator<GetAdminProvidersQuery>
{
    private static readonly string[] ValidSortByValues = ["hierarchy", "tier", "listings", "createdat", "updatedat"];

    public GetAdminProvidersQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);

        RuleFor(x => x.SortBy)
            .Must(v => v is null || ValidSortByValues.Contains(v.ToLowerInvariant()))
            .WithMessage($"sortBy must be one of: {string.Join(", ", ValidSortByValues)}");

        RuleFor(x => x.SortDir)
            .Must(v => v is null
                || string.Equals(v, "asc", StringComparison.OrdinalIgnoreCase)
                || string.Equals(v, "desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("sortDir must be 'asc' or 'desc'");
    }
}
