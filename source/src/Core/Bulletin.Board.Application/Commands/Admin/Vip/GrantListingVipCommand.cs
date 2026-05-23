using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Commands.Admin.Vip;

public record GrantListingVipCommand(
    Guid ListingId,
    DateTimeOffset? Until,
    bool Indefinite,
    string? Reason,
    string ReauthToken,
    string? IpAddress) : IRequest;

public sealed class GrantListingVipCommandHandler(
    IRepository<Listing> listingRepository,
    IRepository<Provider> providerRepository,
    IRepository<VipChangeLog> logRepository,
    IReauthTokenService reauthTokenService,
    ICurrentUserService currentUserService,
    ILogger<GrantListingVipCommandHandler> logger)
    : IRequestHandler<GrantListingVipCommand>
{
    public async Task Handle(GrantListingVipCommand request, CancellationToken ct)
    {
        var adminId = currentUserService.UserId ?? throw new UnauthorizedAccessException();

        if (!reauthTokenService.ValidateAndConsume(request.ReauthToken, adminId))
            throw new UnauthorizedAccessException("Invalid or expired re-auth token.");

        var listing = await listingRepository.GetByIdAsync(request.ListingId, ct)
            ?? throw new InvalidOperationException($"Listing '{request.ListingId}' not found.");

        var provider = await providerRepository.GetByIdAsync(listing.ProviderId, ct)
            ?? throw new InvalidOperationException($"Provider '{listing.ProviderId}' not found.");

        if (provider.Tier != ProviderTier.VIP || !provider.IsVipNow(DateTimeOffset.UtcNow))
            throw new InvalidOperationException("Provider must be VIP before any of its listings can be VIP.");

        var prevUntil = listing.VipUntil;
        var prevIndef = listing.VipIsIndefinite;
        var wasVip = listing.ProviderTier == ProviderTier.VIP;

        listing.GrantVip(provider.Tier, request.Until, request.Indefinite, DateTimeOffset.UtcNow);
        listingRepository.Update(listing);

        await logRepository.AddAsync(VipChangeLog.Record(
            VipEntityType.Listing, listing.Id,
            wasVip ? VipChangeAction.Extended : VipChangeAction.Granted,
            prevUntil, prevIndef,
            listing.VipUntil, listing.VipIsIndefinite,
            adminId, request.IpAddress, request.Reason), ct);

        await listingRepository.SaveChangesAsync(ct);

        logger.LogInformation("Listing {ListingId} VIP granted by {AdminId} (indefinite={Indef}, until={Until})",
            listing.Id, adminId, request.Indefinite, request.Until);
    }
}

public sealed class GrantListingVipCommandValidator : AbstractValidator<GrantListingVipCommand>
{
    public GrantListingVipCommandValidator()
    {
        RuleFor(x => x.ListingId).NotEmpty();
        RuleFor(x => x.ReauthToken).NotEmpty();
        RuleFor(x => x).Must(x => x.Indefinite ^ x.Until.HasValue)
            .WithMessage("Specify either an expiration date or indefinite, not both.");
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}
