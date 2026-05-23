using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace Bulletin.Board.Application.Commands.Listings;

/// <summary>
/// Records a public-facing view event for a listing. Throttled in-process per
/// (listingId, sessionHash) for 5 minutes to absorb refresh hammering — the DB still
/// stays accurate enough for analytics + trending score because real users do not
/// reopen the same listing dozens of times per hour from the same session.
/// </summary>
public record RecordListingViewCommand(Guid ListingId, string SessionHash, Guid? UserId)
    : IRequest;

public sealed class RecordListingViewCommandHandler(
    IListingRepository listings,
    IRepository<ListingView> views,
    IMemoryCache cache)
    : IRequestHandler<RecordListingViewCommand>
{
    private static readonly TimeSpan ThrottleWindow = TimeSpan.FromMinutes(5);

    public async Task Handle(RecordListingViewCommand request, CancellationToken ct)
    {
        var cacheKey = $"view:{request.ListingId:N}:{request.SessionHash}";
        if (cache.TryGetValue(cacheKey, out _))
            return; // Recent duplicate from this session — silently drop.

        // Verify the listing exists and is publicly viewable (Approved, not deleted, not paused).
        // We do not want stalkers using this endpoint as an enumeration oracle, so we treat
        // non-viewable listings as "view recorded" silently rather than 404.
        var listing = await listings.GetByIdAsync(request.ListingId, ct);
        if (listing is null || listing.IsDeleted || listing.IsPausedByOwner
            || listing.Status != ListingStatus.Approved)
        {
            cache.Set(cacheKey, true, ThrottleWindow);
            return;
        }

        var view = ListingView.Create(request.ListingId, request.SessionHash, request.UserId);
        await views.AddAsync(view, ct);
        await views.SaveChangesAsync(ct);

        cache.Set(cacheKey, true, ThrottleWindow);
    }
}

public sealed class RecordListingViewCommandValidator : AbstractValidator<RecordListingViewCommand>
{
    public RecordListingViewCommandValidator()
    {
        RuleFor(x => x.ListingId).NotEmpty();
        RuleFor(x => x.SessionHash).NotEmpty().Length(8, 64);
    }
}
