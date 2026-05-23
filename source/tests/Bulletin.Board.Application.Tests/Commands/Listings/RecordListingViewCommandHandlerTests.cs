using Bulletin.Board.Application.Commands.Listings;
using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;

namespace Bulletin.Board.Application.Tests.Commands.Listings;

public class RecordListingViewCommandHandlerTests
{
    private readonly IListingRepository _listings = Substitute.For<IListingRepository>();
    private readonly IRepository<ListingView> _views = Substitute.For<IRepository<ListingView>>();
    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    private readonly RecordListingViewCommandHandler _sut;

    public RecordListingViewCommandHandlerTests()
    {
        _sut = new RecordListingViewCommandHandler(_listings, _views, _cache);
    }

    [Fact]
    public async Task Inserts_view_when_listing_is_approved_and_session_not_cached()
    {
        var listing = CreateApprovedListing();
        _listings.GetByIdAsync(listing.Id, Arg.Any<CancellationToken>()).Returns(listing);

        await _sut.Handle(new RecordListingViewCommand(listing.Id, "session-abc", null), default);

        await _views.Received(1).AddAsync(
            Arg.Is<ListingView>(v => v.ListingId == listing.Id && v.SessionHash == "session-abc"),
            Arg.Any<CancellationToken>());
        await _views.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Throttle_blocks_second_view_from_same_session_within_window()
    {
        var listing = CreateApprovedListing();
        _listings.GetByIdAsync(listing.Id, Arg.Any<CancellationToken>()).Returns(listing);

        await _sut.Handle(new RecordListingViewCommand(listing.Id, "same-session", null), default);
        await _sut.Handle(new RecordListingViewCommand(listing.Id, "same-session", null), default);

        // Only the first call should reach the repository — second is short-circuited by cache.
        await _views.Received(1).AddAsync(Arg.Any<ListingView>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Throttle_is_scoped_per_listing_and_session()
    {
        var listingA = CreateApprovedListing();
        var listingB = CreateApprovedListing();
        _listings.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<Guid>() == listingA.Id ? listingA : listingB);

        // Same session, different listings → both views persist.
        await _sut.Handle(new RecordListingViewCommand(listingA.Id, "session-x", null), default);
        await _sut.Handle(new RecordListingViewCommand(listingB.Id, "session-x", null), default);

        await _views.Received(2).AddAsync(Arg.Any<ListingView>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Silently_drops_view_for_deleted_listing()
    {
        var listing = CreateApprovedListing();
        listing.SoftDelete();
        _listings.GetByIdAsync(listing.Id, Arg.Any<CancellationToken>()).Returns(listing);

        await _sut.Handle(new RecordListingViewCommand(listing.Id, "session-y", null), default);

        await _views.DidNotReceive().AddAsync(Arg.Any<ListingView>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Silently_drops_view_for_paused_listing()
    {
        var listing = CreateApprovedListing();
        listing.PauseByOwner();
        _listings.GetByIdAsync(listing.Id, Arg.Any<CancellationToken>()).Returns(listing);

        await _sut.Handle(new RecordListingViewCommand(listing.Id, "session-z", null), default);

        await _views.DidNotReceive().AddAsync(Arg.Any<ListingView>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Silently_drops_view_for_missing_listing()
    {
        _listings.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Listing?)null);

        await _sut.Handle(new RecordListingViewCommand(Guid.NewGuid(), "session-q", null), default);

        await _views.DidNotReceive().AddAsync(Arg.Any<ListingView>(), Arg.Any<CancellationToken>());
    }

    private static Listing CreateApprovedListing()
    {
        var listing = Listing.Create(
            providerId: Guid.NewGuid(),
            categoryId: Guid.NewGuid(),
            providerTier: ProviderTier.VIP,
            titleEn: "Test Listing",
            titleEs: "Listing de prueba");
        listing.Approve(Guid.NewGuid());
        return listing;
    }
}
