using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Commands.Admin.Providers;

public enum VerificationFlagKind
{
    Phone,
    Identity,
    Social
}

public record SetVerificationFlagCommand(Guid ProviderId, VerificationFlagKind Kind) : IRequest;

public sealed class SetVerificationFlagCommandHandler(
    IRepository<Provider> providerRepository,
    IRepository<Verification> verificationRepository,
    ICurrentUserService currentUserService,
    ILogger<SetVerificationFlagCommandHandler> logger)
    : IRequestHandler<SetVerificationFlagCommand>
{
    public async Task Handle(SetVerificationFlagCommand request, CancellationToken ct)
    {
        var provider = await providerRepository.GetByIdAsync(request.ProviderId, ct)
            ?? throw new InvalidOperationException($"Provider '{request.ProviderId}' not found.");

        var adminId = currentUserService.UserId ?? throw new UnauthorizedAccessException();

        var verification = (await verificationRepository.FindAsync(
            v => v.ProviderId == provider.Id, ct)).FirstOrDefault();

        if (verification is null)
        {
            verification = Verification.Create(provider.Id);
            await verificationRepository.AddAsync(verification, ct);
        }

        switch (request.Kind)
        {
            case VerificationFlagKind.Phone:
                verification.VerifyPhone();
                break;
            case VerificationFlagKind.Identity:
                verification.ApproveIdentity();
                break;
            case VerificationFlagKind.Social:
                verification.VerifySocial();
                break;
        }

        verificationRepository.Update(verification);
        await verificationRepository.SaveChangesAsync(ct);

        logger.LogInformation(
            "Admin {AdminId} set {Kind} verified for provider {ProviderId}",
            adminId, request.Kind, request.ProviderId);
    }
}

public sealed class SetVerificationFlagCommandValidator : AbstractValidator<SetVerificationFlagCommand>
{
    public SetVerificationFlagCommandValidator()
    {
        RuleFor(x => x.ProviderId).NotEmpty();
        RuleFor(x => x.Kind).IsInEnum();
    }
}
