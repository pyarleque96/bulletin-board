using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Commands.Admin.Vip;

public record RevokeProviderVipCommand(
    Guid ProviderId,
    string? Reason,
    string ReauthToken,
    string? IpAddress) : IRequest;

public sealed class RevokeProviderVipCommandHandler(
    IRepository<Provider> providerRepository,
    IRepository<Listing> listingRepository,
    IRepository<VipChangeLog> logRepository,
    IReauthTokenService reauthTokenService,
    ICurrentUserService currentUserService,
    ILogger<RevokeProviderVipCommandHandler> logger)
    : IRequestHandler<RevokeProviderVipCommand>
{
    public async Task Handle(RevokeProviderVipCommand request, CancellationToken ct)
    {
        var adminId = currentUserService.UserId ?? throw new UnauthorizedAccessException();

        if (!reauthTokenService.ValidateAndConsume(request.ReauthToken, adminId))
            throw new UnauthorizedAccessException("Invalid or expired re-auth token.");

        var provider = await providerRepository.GetByIdAsync(request.ProviderId, ct)
            ?? throw new InvalidOperationException($"Provider '{request.ProviderId}' not found.");

        var prevUntil = provider.VipUntil;
        var prevIndef = provider.VipIsIndefinite;

        provider.RevokeVip();
        providerRepository.Update(provider);

        await logRepository.AddAsync(VipChangeLog.Record(
            VipEntityType.Provider, provider.Id,
            VipChangeAction.Revoked,
            prevUntil, prevIndef,
            null, false,
            adminId, request.IpAddress, request.Reason), ct);

        var listings = await listingRepository.FindAsync(l => l.ProviderId == provider.Id && !l.IsDeleted, ct);
        foreach (var listing in listings)
        {
            var lprev = listing.VipUntil;
            var lprevIndef = listing.VipIsIndefinite;
            listing.RevokeVip(provider.Tier);
            listingRepository.Update(listing);

            await logRepository.AddAsync(VipChangeLog.Record(
                VipEntityType.Listing, listing.Id,
                VipChangeAction.Revoked,
                lprev, lprevIndef,
                null, false,
                adminId, request.IpAddress, request.Reason), ct);
        }

        await providerRepository.SaveChangesAsync(ct);

        logger.LogInformation("Provider {ProviderId} VIP revoked by {AdminId} (listings cascaded={Count})",
            provider.Id, adminId, listings.Count);
    }
}

public sealed class RevokeProviderVipCommandValidator : AbstractValidator<RevokeProviderVipCommand>
{
    public RevokeProviderVipCommandValidator()
    {
        RuleFor(x => x.ProviderId).NotEmpty();
        RuleFor(x => x.ReauthToken).NotEmpty();
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}
