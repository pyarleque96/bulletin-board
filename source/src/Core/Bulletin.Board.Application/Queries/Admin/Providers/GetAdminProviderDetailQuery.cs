using Bulletin.Board.Application.DTOs.Admin;
using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Queries.Admin.Providers;

public record GetAdminProviderDetailQuery(Guid ProviderId) : IRequest<AdminProviderDetailDto?>;

public sealed class GetAdminProviderDetailQueryHandler(
    IRepository<Provider> providerRepository,
    IRepository<Verification> verificationRepository,
    IListingRepository listingRepository,
    ILogger<GetAdminProviderDetailQueryHandler> logger)
    : IRequestHandler<GetAdminProviderDetailQuery, AdminProviderDetailDto?>
{
    public async Task<AdminProviderDetailDto?> Handle(GetAdminProviderDetailQuery request, CancellationToken ct)
    {
        logger.LogInformation("Admin fetching provider detail for {ProviderId}", request.ProviderId);

        var provider = await providerRepository.GetByIdAsync(request.ProviderId, ct);
        if (provider is null)
            return null;

        var verification = (await verificationRepository.FindAsync(
            v => v.ProviderId == request.ProviderId, ct)).FirstOrDefault();

        var listings = await listingRepository.FindAsync(
            l => l.ProviderId == request.ProviderId && !l.IsDeleted, ct);

        var now = DateTimeOffset.UtcNow;
        var listingDtos = listings
            .OrderByDescending(l => l.UpdatedAt)
            .Select(l => new AdminListingDto(
                Id: l.Id,
                TitleEn: l.TitleEn,
                TitleEs: l.TitleEs,
                ProviderName: provider.WhatsAppNumber,
                ProviderPhone: provider.WhatsAppNumber,
                ProviderTier: l.ProviderTier.ToString(),
                CategoryName: l.CategoryId.ToString(),
                Status: l.Status.ToString(),
                CreatedAt: l.CreatedAt,
                UpdatedAt: l.UpdatedAt,
                AvgRating: l.AvgRating,
                PhotoCount: 0,
                IsVipNow: l.IsVipNow(now),
                VipUntil: l.VipUntil,
                VipIsIndefinite: l.VipIsIndefinite))
            .ToList();

        return new AdminProviderDetailDto(
            Id: provider.Id,
            UserId: provider.UserId,
            Tier: provider.Tier.ToString(),
            VerificationStatus: provider.VerificationStatus.ToString(),
            WhatsAppNumber: provider.WhatsAppNumber,
            Bio: provider.Bio,
            Hierarchy: provider.Hierarchy,
            CreatedAt: provider.CreatedAt,
            VerifiedAt: provider.VerifiedAt,
            PhoneVerified: verification?.PhoneVerifiedAt.HasValue ?? false,
            IdentityVerified: verification?.IdentityStatus == VerificationStatus.Approved,
            SocialVerified: verification?.SocialVerifiedAt.HasValue ?? false,
            Listings: listingDtos);
    }
}
