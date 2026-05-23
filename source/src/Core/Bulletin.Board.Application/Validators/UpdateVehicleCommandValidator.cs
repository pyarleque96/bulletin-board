using Bulletin.Board.Application.Commands.Vehicles;
using FluentValidation;

namespace Bulletin.Board.Application.Validators;

public sealed class UpdateVehicleCommandValidator : AbstractValidator<UpdateVehicleCommand>
{
    private static readonly string[] AllowedTransmissions = ["Manual", "Automatic", "Both"];

    public UpdateVehicleCommandValidator()
    {
        RuleFor(x => x.VehicleId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(80);
        RuleFor(x => x.PassengerMax).GreaterThanOrEqualTo(1).LessThanOrEqualTo(50);
        RuleFor(x => x.Transmission).Must(t => AllowedTransmissions.Contains(t, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Transmission must be one of: {string.Join(", ", AllowedTransmissions)}.");
        RuleFor(x => x.DailyRateCents).GreaterThan(0);
        RuleFor(x => x.WeeklyRateCents).GreaterThan(0).When(x => x.WeeklyRateCents.HasValue);
        RuleFor(x => x.MonthlyRateCents).GreaterThan(0).When(x => x.MonthlyRateCents.HasValue);
    }
}
