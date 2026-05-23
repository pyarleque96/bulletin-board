using Bulletin.Board.Application.Commands.Auth;
using FluentValidation;

namespace Bulletin.Board.Application.Validators;

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    private static readonly string[] AllowedRoles = ["User", "Provider"];

    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required.")
            .Must(r => AllowedRoles.Contains(r)).WithMessage("Role must be 'User' or 'Provider'.");
    }
}
