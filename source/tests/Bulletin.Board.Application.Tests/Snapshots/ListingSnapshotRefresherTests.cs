using Bulletin.Board.Application.Snapshots;
using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Interfaces.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Bulletin.Board.Application.Tests.Snapshots;

public class ListingSnapshotRefresherTests
{
    private readonly IListingRepository _repo = Substitute.For<IListingRepository>();
    private readonly IListingSnapshotBuilder _builder = Substitute.For<IListingSnapshotBuilder>();
    private readonly ListingSnapshotRefresher _sut;

    public ListingSnapshotRefresherTests()
    {
        _sut = new ListingSnapshotRefresher(
            _repo, _builder, NullLogger<ListingSnapshotRefresher>.Instance);
    }

    [Fact]
    public async Task RefreshIfApproved_returns_false_when_listing_not_found()
    {
        _repo.GetForSnapshotAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Listing?)null);

        var result = await _sut.RefreshIfApprovedAsync(Guid.NewGuid());

        result.Should().BeFalse();
        _builder.DidNotReceive().Build(Arg.Any<Listing>());
    }

    [Fact]
    public async Task RefreshIfApproved_is_noop_when_listing_is_pending()
    {
        var listing = MakeListing(approve: false);
        _repo.GetForSnapshotAsync(listing.Id, Arg.Any<CancellationToken>()).Returns(listing);

        var result = await _sut.RefreshIfApprovedAsync(listing.Id);

        result.Should().BeFalse();
        _builder.DidNotReceive().Build(Arg.Any<Listing>());
    }

    [Fact]
    public async Task RefreshIfApproved_is_noop_when_listing_soft_deleted()
    {
        var listing = MakeListing(approve: true);
        listing.SoftDelete();
        _repo.GetForSnapshotAsync(listing.Id, Arg.Any<CancellationToken>()).Returns(listing);

        var result = await _sut.RefreshIfApprovedAsync(listing.Id);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task RefreshIfApproved_writes_snapshot_when_listing_is_approved()
    {
        var listing = MakeListing(approve: true);
        _repo.GetForSnapshotAsync(listing.Id, Arg.Any<CancellationToken>()).Returns(listing);
        _builder.Build(listing).Returns("{\"version\":1,\"x\":1}");

        var result = await _sut.RefreshIfApprovedAsync(listing.Id);

        result.Should().BeTrue();
        listing.PublishedSnapshot.Should().Be("{\"version\":1,\"x\":1}");
        listing.PublishedSnapshotVersion.Should().Be(ListingSnapshot.CurrentVersion);
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task BackfillMissing_returns_zero_when_no_listings_missing_snapshot()
    {
        _repo.GetIdsMissingSnapshotAsync(Arg.Any<CancellationToken>()).Returns(new List<Guid>());

        var count = await _sut.BackfillMissingAsync();

        count.Should().Be(0);
        await _repo.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task BackfillMissing_processes_each_id_and_saves_once()
    {
        var listing1 = MakeListing(approve: true);
        var listing2 = MakeListing(approve: true);
        _repo.GetIdsMissingSnapshotAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Guid> { listing1.Id, listing2.Id });
        _repo.GetForSnapshotAsync(listing1.Id, Arg.Any<CancellationToken>()).Returns(listing1);
        _repo.GetForSnapshotAsync(listing2.Id, Arg.Any<CancellationToken>()).Returns(listing2);
        _builder.Build(Arg.Any<Listing>()).Returns("{\"version\":1}");

        var count = await _sut.BackfillMissingAsync();

        count.Should().Be(2);
        listing1.PublishedSnapshot.Should().NotBeNull();
        listing2.PublishedSnapshot.Should().NotBeNull();
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>()); // single batch save
    }

    [Fact]
    public async Task BackfillMissing_skips_listings_that_changed_state_between_query_and_load()
    {
        // Race: id was Approved when GetIds returned, but somebody Reject'd before we loaded it.
        var listing = MakeListing(approve: false);
        _repo.GetIdsMissingSnapshotAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Guid> { listing.Id });
        _repo.GetForSnapshotAsync(listing.Id, Arg.Any<CancellationToken>()).Returns(listing);

        var count = await _sut.BackfillMissingAsync();

        count.Should().Be(0);
        listing.PublishedSnapshot.Should().BeNull();
    }

    private static Listing MakeListing(bool approve)
    {
        var l = Listing.Create(
            providerId: Guid.NewGuid(),
            categoryId: Guid.NewGuid(),
            providerTier: ProviderTier.VIP,
            titleEn: "T", titleEs: "T");
        if (approve) l.Approve(Guid.NewGuid());
        return l;
    }
}
