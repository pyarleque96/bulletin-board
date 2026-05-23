using Bulletin.Board.Application.Commands.Reservations;
using FluentValidation;

namespace Bulletin.Board.Application.Validators;

public sealed class CreateReservationRequestCommandValidator
    : AbstractValidator<CreateReservationRequestCommand>
{
    public CreateReservationRequestCommandValidator()
    {
        RuleFor(x => x.VehicleId).NotEmpty();
        RuleFor(x => x.StartDate).NotEqual(default(DateOnly));
        RuleFor(x => x.EndDate)
            .NotEqual(default(DateOnly))
            .GreaterThanOrEqualTo(x => x.StartDate);
        RuleFor(x => x.RequesterName).NotEmpty().MaximumLength(160);
        RuleFor(x => x.RequesterEmail).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.RequesterPhone).MaximumLength(30).When(x => x.RequesterPhone is not null);
        RuleFor(x => x.Comment).MaximumLength(1000).When(x => x.Comment is not null);
    }
}
