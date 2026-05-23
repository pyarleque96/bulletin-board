using Bulletin.Board.Application.Queries.Listings;
using Bulletin.Board.Application.Snapshots;
using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Interfaces.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Bulletin.Board.Application.Tests.Queries.Listings;

public class GetListingDetailBySlugQueryHandlerTests
{
    private readonly IListingRepository _repo = Substitute.For<IListingRepository>();
    private readonly IListingSnapshotBuilder _snapshot = Substitute.For<IListingSnapshotBuilder>();
    private readonly GetListingDetailBySlugQueryHandler _sut;

    public GetListingDetailBySlugQueryHandlerTests()
    {
        _sut = new GetListingDetailBySlugQueryHandler(
            _repo,
            _snapshot,
            NullLogger<GetListingDetailBySlugQueryHandler>.Instance);
    }

    [Fact]
    public async Task Returns_null_when_repo_finds_no_listing()
    {
        _repo.GetDetailBySlugAsync("missing-slug", Arg.Any<CancellationToken>())
            .Returns((Listing?)null);

        var result = await _sut.Handle(new GetListingDetailBySlugQuery("missing-slug"), default);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Returns_null_when_listing_is_soft_deleted()
    {
        var listing = CreateApprovedListing();
        listing.SoftDelete();
        _repo.GetDetailBySlugAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(listing);

        var result = await _sut.Handle(new GetListingDetailBySlugQuery("any"), default);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Returns_null_when_listing_is_not_approved()
    {
        var listing = CreateListing(approve: false);
        _repo.GetDetailBySlugAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(listing);

        var result = await _sut.Handle(new GetListingDetailBySlugQuery("any"), default);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Returns_dto_with_core_fields_when_listing_is_approved()
    {
        var listing = CreateApprovedListing();
        _repo.GetDetailBySlugAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(listing);
        _repo.GetRankedAsync(Arg.Any<Guid?>(), Arg.Any<ListingStatus>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var result = await _sut.Handle(new GetListingDetailBySlugQuery("dells-premier-car-rentals"), default);

        result.Should().NotBeNull();
        result!.Id.Should().Be(listing.Id);
        result.TitleEn.Should().Be("Dells Premier Car Rentals");
        result.Tier.Should().Be("VIP");
        result.Status.Should().Be("Approved");
    }

    [Fact]
    public async Task Normalizes_slug_before_repo_lookup()
    {
        // Caller passes "Foo BAR" — handler must normalize to "foo-bar" before hitting the repo,
        // so the unique slug index resolves the same row regardless of input casing/whitespace.
        _repo.GetDetailBySlugAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((Listing?)null);

        await _sut.Handle(new GetListingDetailBySlugQuery("Foo BAR"), default);

        await _repo.Received(1).GetDetailBySlugAsync("foo-bar", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Returns_dto_from_snapshot_when_listing_is_pending_but_has_snapshot()
    {
        // Listing was approved earlier (snapshot captured), now provider edited and it's back
        // to Pending. Public should still see the snapshot version.
        var listing = CreateListing(approve: false);
        listing.Approve(Guid.NewGuid(), snapshotJson: "{\"version\":1}");
        listing.Edit("Edited Title", "Editado", null, null, null, null); // flips back to Pending
        listing.Status.Should().Be(ListingStatus.Pending);
        listing.PublishedSnapshot.Should().NotBeNull();

        _repo.GetDetailBySlugAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(listing);
        _repo.GetRankedAsync(Arg.Any<Guid?>(), Arg.Any<ListingStatus>(),
            Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns([]);

        // Snapshot parser returns a populated snapshot — mapper should project from it.
        _snapshot.Parse(Arg.Any<string>()).Returns(new ListingSnapshot(
            Version: 1,
            SnapshotAt: DateTimeOffset.UtcNow,
            TitleEn: "Frozen Title",
            TitleEs: "Titulo Congelado",
            DescriptionEn: null, DescriptionEs: null,
            Price: null, PriceLabelEn: null, PriceLabelEs: null,
            Location: null, WhatsAppNumber: null,
            Images: [], Vehicles: []));

        var result = await _sut.Handle(new GetListingDetailBySlugQuery("any"), default);

        result.Should().NotBeNull();
        result!.TitleEn.Should().Be("Frozen Title");      // from snapshot, not the edited live row
        result.Status.Should().Be("Pending");             // status reflects current entity
    }

    [Fact]
    public async Task Returns_null_when_listing_is_soft_deleted_even_with_snapshot()
    {
        // Soft-delete must always hide a listing — snapshot doesn't override admin removal.
        var listing = CreateListing(approve: false);
        listing.Approve(Guid.NewGuid(), snapshotJson: "{\"version\":1}");
        listing.SoftDelete();

        _repo.GetDetailBySlugAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(listing);

        var result = await _sut.Handle(new GetListingDetailBySlugQuery("any"), default);

        result.Should().BeNull();
    }

    private static Listing CreateApprovedListing()
    {
        var listing = CreateListing(approve: false);
        listing.Approve(Guid.NewGuid());
        return listing;
    }

    private static Listing CreateListing(bool approve)
    {
        var listing = Listing.Create(
            providerId: Guid.NewGuid(),
            categoryId: Guid.NewGuid(),
            providerTier: ProviderTier.VIP,
            titleEn: "Dells Premier Car Rentals",
            titleEs: "Dells Rentas de Autos Premium");

        if (approve) listing.Approve(Guid.NewGuid());
        return listing;
    }
}
