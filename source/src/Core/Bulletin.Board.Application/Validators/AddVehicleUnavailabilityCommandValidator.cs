using Bulletin.Board.Application.Commands.Vehicles.Availability;
using FluentValidation;

namespace Bulletin.Board.Application.Validators;

public sealed class AddVehicleUnavailabilityCommandValidator
    : AbstractValidator<AddVehicleUnavailabilityCommand>
{
    private static readonly string[] AllowedReasons = ["Manual", "Maintenance"];

    public AddVehicleUnavailabilityCommandValidator()
    {
        RuleFor(x => x.VehicleId).NotEmpty();
        RuleFor(x => x.StartDate).NotEqual(default(DateOnly));
        RuleFor(x => x.EndDate)
            .NotEqual(default(DateOnly))
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("End date must be on or after start date.");
        RuleFor(x => x.Reason)
            .Must(r => AllowedReasons.Contains(r, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Reason must be one of: {string.Join(", ", AllowedReasons)}.");
    }
}
