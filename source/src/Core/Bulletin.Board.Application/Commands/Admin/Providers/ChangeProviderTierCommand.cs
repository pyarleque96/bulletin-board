using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Commands.Admin.Providers;

public record ChangeProviderTierCommand(Guid ProviderId, string Tier) : IRequest;

public sealed class ChangeProviderTierCommandHandler(
    IRepository<Provider> providerRepository,
    ICurrentUserService currentUserService,
    ILogger<ChangeProviderTierCommandHandler> logger)
    : IRequestHandler<ChangeProviderTierCommand>
{
    public async Task Handle(ChangeProviderTierCommand request, CancellationToken ct)
    {
        var provider = await providerRepository.GetByIdAsync(request.ProviderId, ct)
            ?? throw new InvalidOperationException($"Provider '{request.ProviderId}' not found.");

        var adminId = currentUserService.UserId ?? throw new UnauthorizedAccessException();

        if (!Enum.TryParse<ProviderTier>(request.Tier, ignoreCase: true, out var tier))
            throw new InvalidOperationException($"Invalid tier value '{request.Tier}'.");

        provider.ApproveTier(tier);
        providerRepository.Update(provider);
        await providerRepository.SaveChangesAsync(ct);

        logger.LogInformation("Provider {ProviderId} tier changed to {Tier} by {AdminId}",
            request.ProviderId, tier, adminId);
    }
}

public sealed class ChangeProviderTierCommandValidator : AbstractValidator<ChangeProviderTierCommand>
{
    public ChangeProviderTierCommandValidator()
    {
        RuleFor(x => x.ProviderId).NotEmpty();
        RuleFor(x => x.Tier).NotEmpty();
    }
}
