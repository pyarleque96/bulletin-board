using Bulletin.Board.Application.Commands.Listings;
using FluentValidation;

namespace Bulletin.Board.Application.Validators;

public sealed class SetListingActiveCommandValidator : AbstractValidator<SetListingActiveCommand>
{
    public SetListingActiveCommandValidator()
    {
        RuleFor(x => x.ListingId).NotEmpty();
    }
}
