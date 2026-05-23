using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Commands.Admin.Vip;

public record RevokeListingVipCommand(
    Guid ListingId,
    string? Reason,
    string ReauthToken,
    string? IpAddress) : IRequest;

public sealed class RevokeListingVipCommandHandler(
    IRepository<Listing> listingRepository,
    IRepository<Provider> providerRepository,
    IRepository<VipChangeLog> logRepository,
    IReauthTokenService reauthTokenService,
    ICurrentUserService currentUserService,
    ILogger<RevokeListingVipCommandHandler> logger)
    : IRequestHandler<RevokeListingVipCommand>
{
    public async Task Handle(RevokeListingVipCommand request, CancellationToken ct)
    {
        var adminId = currentUserService.UserId ?? throw new UnauthorizedAccessException();

        if (!reauthTokenService.ValidateAndConsume(request.ReauthToken, adminId))
            throw new UnauthorizedAccessException("Invalid or expired re-auth token.");

        var listing = await listingRepository.GetByIdAsync(request.ListingId, ct)
            ?? throw new InvalidOperationException($"Listing '{request.ListingId}' not found.");

        var provider = await providerRepository.GetByIdAsync(listing.ProviderId, ct)
            ?? throw new InvalidOperationException($"Provider '{listing.ProviderId}' not found.");

        var prevUntil = listing.VipUntil;
        var prevIndef = listing.VipIsIndefinite;

        var fallbackTier = provider.Tier == ProviderTier.VIP
            ? ProviderTier.VIP
            : provider.Tier;

        listing.RevokeVip(fallbackTier);
        listingRepository.Update(listing);

        await logRepository.AddAsync(VipChangeLog.Record(
            VipEntityType.Listing, listing.Id,
            VipChangeAction.Revoked,
            prevUntil, prevIndef,
            null, false,
            adminId, request.IpAddress, request.Reason), ct);

        await listingRepository.SaveChangesAsync(ct);

        logger.LogInformation("Listing {ListingId} VIP revoked by {AdminId}", listing.Id, adminId);
    }
}

public sealed class RevokeListingVipCommandValidator : AbstractValidator<RevokeListingVipCommand>
{
    public RevokeListingVipCommandValidator()
    {
        RuleFor(x => x.ListingId).NotEmpty();
        RuleFor(x => x.ReauthToken).NotEmpty();
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}
