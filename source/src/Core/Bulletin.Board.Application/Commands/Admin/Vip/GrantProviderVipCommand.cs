using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Commands.Admin.Vip;

public record GrantProviderVipCommand(
    Guid ProviderId,
    DateTimeOffset? Until,
    bool Indefinite,
    string? Reason,
    string ReauthToken,
    string? IpAddress) : IRequest;

public sealed class GrantProviderVipCommandHandler(
    IRepository<Provider> providerRepository,
    IRepository<Listing> listingRepository,
    IRepository<VipChangeLog> logRepository,
    IReauthTokenService reauthTokenService,
    ICurrentUserService currentUserService,
    ILogger<GrantProviderVipCommandHandler> logger)
    : IRequestHandler<GrantProviderVipCommand>
{
    public async Task Handle(GrantProviderVipCommand request, CancellationToken ct)
    {
        var adminId = currentUserService.UserId ?? throw new UnauthorizedAccessException();

        if (!reauthTokenService.ValidateAndConsume(request.ReauthToken, adminId))
            throw new UnauthorizedAccessException("Invalid or expired re-auth token.");

        var provider = await providerRepository.GetByIdAsync(request.ProviderId, ct)
            ?? throw new InvalidOperationException($"Provider '{request.ProviderId}' not found.");

        var prevUntil = provider.VipUntil;
        var prevIndef = provider.VipIsIndefinite;
        var wasVip = provider.Tier == ProviderTier.VIP;

        provider.GrantVip(request.Until, request.Indefinite, DateTimeOffset.UtcNow);
        providerRepository.Update(provider);

        await logRepository.AddAsync(VipChangeLog.Record(
            VipEntityType.Provider, provider.Id,
            wasVip ? VipChangeAction.Extended : VipChangeAction.Granted,
            prevUntil, prevIndef,
            provider.VipUntil, provider.VipIsIndefinite,
            adminId, request.IpAddress, request.Reason), ct);

        var listings = await listingRepository.FindAsync(l => l.ProviderId == provider.Id && !l.IsDeleted, ct);
        foreach (var listing in listings)
        {
            var lprev = listing.VipUntil;
            var lprevIndef = listing.VipIsIndefinite;
            listing.InheritVipFromProvider(provider.VipUntil, provider.VipIsIndefinite);
            listingRepository.Update(listing);

            await logRepository.AddAsync(VipChangeLog.Record(
                VipEntityType.Listing, listing.Id,
                VipChangeAction.InheritedFromProvider,
                lprev, lprevIndef,
                listing.VipUntil, listing.VipIsIndefinite,
                adminId, request.IpAddress, request.Reason), ct);
        }

        await providerRepository.SaveChangesAsync(ct);

        logger.LogInformation("Provider {ProviderId} VIP granted by {AdminId} (indefinite={Indef}, until={Until}, listings cascaded={Count})",
            provider.Id, adminId, request.Indefinite, request.Until, listings.Count);
    }
}

public sealed class GrantProviderVipCommandValidator : AbstractValidator<GrantProviderVipCommand>
{
    public GrantProviderVipCommandValidator()
    {
        RuleFor(x => x.ProviderId).NotEmpty();
        RuleFor(x => x.ReauthToken).NotEmpty();
        RuleFor(x => x).Must(x => x.Indefinite ^ x.Until.HasValue)
            .WithMessage("Specify either an expiration date or indefinite, not both.");
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}
