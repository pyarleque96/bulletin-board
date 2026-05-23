using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Application.DTOs.Admin;
using Bulletin.Board.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Queries.Admin.Ratings;

public record GetAdminRatingsQuery(string? Status, int Page, int PageSize) : IRequest<PagedResult<AdminRatingDto>>;

public sealed class GetAdminRatingsQueryHandler(
    IAdminRatingsRepository ratingsRepository,
    ILogger<GetAdminRatingsQueryHandler> logger)
    : IRequestHandler<GetAdminRatingsQuery, PagedResult<AdminRatingDto>>
{
    public async Task<PagedResult<AdminRatingDto>> Handle(GetAdminRatingsQuery request, CancellationToken ct)
    {
        logger.LogInformation("Admin fetching ratings with status={Status}", request.Status);

        var (items, total) = await ratingsRepository.GetPagedAsync(request.Status, request.Page, request.PageSize, ct);

        var paged = items.Select(r => new AdminRatingDto(
            Id: r.Id,
            ListingTitleEn: r.ListingTitleEn,
            ProviderName: r.ProviderWhatsApp,
            ReviewerName: r.ReviewerName,
            ReviewerEmail: r.ReviewerEmail,
            Stars: r.Stars,
            Comment: r.Comment,
            Status: r.Status,
            CreatedAt: r.CreatedAt,
            ContactDate: r.ContactDate))
            .ToList();

        return new PagedResult<AdminRatingDto>(paged, total, request.Page, request.PageSize);
    }
}

public sealed class GetAdminRatingsQueryValidator : AbstractValidator<GetAdminRatingsQuery>
{
    public GetAdminRatingsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
