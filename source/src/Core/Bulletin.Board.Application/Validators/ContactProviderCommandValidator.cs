using Bulletin.Board.Application.Commands.Contacts;
using FluentValidation;

namespace Bulletin.Board.Application.Validators;

public sealed class ContactProviderCommandValidator : AbstractValidator<ContactProviderCommand>
{
    public ContactProviderCommandValidator()
    {
        RuleFor(x => x.ListingId)
            .NotEmpty().WithMessage("ListingId is required.");

        RuleFor(x => x.WaiverAccepted)
            .Must(v => v).WithMessage("You must accept the waiver to contact a provider.");
    }
}
