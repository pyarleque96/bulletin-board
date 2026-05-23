using Bulletin.Board.Domain.Interfaces.Services;
using FluentValidation;
using MediatR;

namespace Bulletin.Board.Application.Commands.Admin.Auth;

public record ReauthCommand(string Password) : IRequest<ReauthResult>;

public record ReauthResult(string Token, DateTimeOffset ExpiresAt);

public sealed class ReauthCommandHandler(
    ICurrentUserService currentUserService,
    IUserService userService,
    IReauthTokenService reauthTokenService)
    : IRequestHandler<ReauthCommand, ReauthResult>
{
    public async Task<ReauthResult> Handle(ReauthCommand request, CancellationToken ct)
    {
        var userId = currentUserService.UserId ?? throw new UnauthorizedAccessException();
        var email = currentUserService.Email ?? throw new UnauthorizedAccessException();

        var (success, _, _, _) = await userService.ValidateCredentialsAsync(email, request.Password, ct);
        if (!success)
            throw new UnauthorizedAccessException("Invalid password.");

        var token = reauthTokenService.IssueToken(userId);
        return new ReauthResult(token, DateTimeOffset.UtcNow.AddMinutes(5));
    }
}

public sealed class ReauthCommandValidator : AbstractValidator<ReauthCommand>
{
    public ReauthCommandValidator()
    {
        RuleFor(x => x.Password).NotEmpty().MinimumLength(1);
    }
}
