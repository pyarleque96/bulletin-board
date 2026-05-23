using Bulletin.Board.Application.Queries.Listings;
using FluentValidation;

namespace Bulletin.Board.Application.Validators;

public sealed class SearchListingsQueryValidator : AbstractValidator<SearchListingsQuery>
{
    public SearchListingsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50).WithMessage("PageSize must be between 1 and 50.");

        RuleFor(x => x.MinRating)
            .InclusiveBetween(1m, 5m).When(x => x.MinRating.HasValue)
            .WithMessage("MinRating must be between 1 and 5.");
    }
}
