using Bulletin.Board.Application.Commands.Listings;
using FluentValidation;

namespace Bulletin.Board.Application.Validators;

public sealed class UpdateOwnListingCommandValidator : AbstractValidator<UpdateOwnListingCommand>
{
    public UpdateOwnListingCommandValidator()
    {
        RuleFor(x => x.ListingId).NotEmpty();
        RuleFor(x => x.TitleEn).NotEmpty().MaximumLength(255);
        RuleFor(x => x.TitleEs).NotEmpty().MaximumLength(255);
        RuleFor(x => x.DescriptionEn).MaximumLength(4000).When(x => x.DescriptionEn is not null);
        RuleFor(x => x.DescriptionEs).MaximumLength(4000).When(x => x.DescriptionEs is not null);
        RuleFor(x => x.Price).GreaterThan(0).When(x => x.Price.HasValue);
        RuleFor(x => x.Location).MaximumLength(255).When(x => x.Location is not null);
        RuleFor(x => x.WhatsAppNumber).MaximumLength(30).When(x => x.WhatsAppNumber is not null);
    }
}
