using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using FluentAssertions;

namespace Bulletin.Board.Domain.Tests.Entities;

public class ListingRequireReapprovalTests
{
    [Fact]
    public void RequireReapproval_reverts_approved_to_pending_and_clears_published_at()
    {
        var listing = MakeApproved();
        listing.Status.Should().Be(ListingStatus.Approved);
        listing.PublishedAt.Should().NotBeNull();

        listing.RequireReapproval();

        listing.Status.Should().Be(ListingStatus.Pending);
        listing.PublishedAt.Should().BeNull();
    }

    [Fact]
    public void RequireReapproval_is_noop_when_already_pending()
    {
        var listing = MakeListing();
        listing.Status.Should().Be(ListingStatus.Pending);

        listing.RequireReapproval();

        listing.Status.Should().Be(ListingStatus.Pending);
    }

    [Fact]
    public void RequireReapproval_does_not_overwrite_NeedsChanges_state()
    {
        var listing = MakeListing();
        listing.RequestChanges("missing photos");
        listing.Status.Should().Be(ListingStatus.NeedsChanges);

        listing.RequireReapproval();

        listing.Status.Should().Be(ListingStatus.NeedsChanges);
    }

    private static Listing MakeApproved()
    {
        var l = MakeListing();
        l.Approve(Guid.NewGuid());
        return l;
    }

    private static Listing MakeListing() => Listing.Create(
        providerId: Guid.NewGuid(),
        categoryId: Guid.NewGuid(),
        providerTier: ProviderTier.VIP,
        titleEn: "Test Listing",
        titleEs: "Anuncio de prueba");
}
