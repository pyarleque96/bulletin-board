using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Commands.Providers;

public sealed class SetProviderHierarchyCommandHandler(
    IRepository<Provider> providerRepository,
    ILogger<SetProviderHierarchyCommandHandler> logger)
    : IRequestHandler<SetProviderHierarchyCommand>
{
    public async Task Handle(SetProviderHierarchyCommand request, CancellationToken ct)
    {
        var provider = await providerRepository.GetByIdAsync(request.ProviderId, ct);
        if (provider is null)
            throw new InvalidOperationException($"Provider '{request.ProviderId}' not found.");

        provider.SetHierarchy(request.Hierarchy);
        providerRepository.Update(provider);
        await providerRepository.SaveChangesAsync(ct);

        logger.LogInformation(
            "Provider {ProviderId} hierarchy set to {Hierarchy}",
            request.ProviderId, request.Hierarchy);
    }
}
