using Bulletin.Board.Application.Commands.Vehicles.Photos;
using FluentValidation;

namespace Bulletin.Board.Application.Validators;

public sealed class AddVehiclePhotoCommandValidator : AbstractValidator<AddVehiclePhotoCommand>
{
    public AddVehiclePhotoCommandValidator()
    {
        RuleFor(x => x.VehicleId).NotEmpty();
        RuleFor(x => x.FilePath)
            .NotEmpty()
            .MaximumLength(500)
            .Must(BeValidUrl).WithMessage("FilePath must be a valid absolute URL (http or https).");
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }

    private static bool BeValidUrl(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath)) return false;

        // Local blob uploads produce absolute paths under /uploads/<container>/file.ext.
        if (filePath.StartsWith("/uploads/", StringComparison.Ordinal)) return true;

        return Uri.TryCreate(filePath, UriKind.Absolute, out var uri)
               && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
