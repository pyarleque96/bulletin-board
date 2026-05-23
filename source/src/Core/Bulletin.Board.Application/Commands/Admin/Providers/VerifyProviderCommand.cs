using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Commands.Admin.Providers;

public record VerifyProviderCommand(Guid ProviderId) : IRequest;

public sealed class VerifyProviderCommandHandler(
    IRepository<Provider> providerRepository,
    ICurrentUserService currentUserService,
    ILogger<VerifyProviderCommandHandler> logger)
    : IRequestHandler<VerifyProviderCommand>
{
    public async Task Handle(VerifyProviderCommand request, CancellationToken ct)
    {
        var provider = await providerRepository.GetByIdAsync(request.ProviderId, ct)
            ?? throw new InvalidOperationException($"Provider '{request.ProviderId}' not found.");

        var adminId = currentUserService.UserId ?? throw new UnauthorizedAccessException();

        provider.ApproveVerification();
        providerRepository.Update(provider);
        await providerRepository.SaveChangesAsync(ct);

        logger.LogInformation("Provider {ProviderId} verified by {AdminId}", request.ProviderId, adminId);
    }
}

public sealed class VerifyProviderCommandValidator : AbstractValidator<VerifyProviderCommand>
{
    public VerifyProviderCommandValidator()
    {
        RuleFor(x => x.ProviderId).NotEmpty();
    }
}
