using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using FluentAssertions;

namespace Bulletin.Board.Domain.Tests.Entities;

public class ListingPauseResumeTests
{
    [Fact]
    public void PauseByOwner_sets_IsPausedByOwner_to_true()
    {
        var listing = MakeListing();
        listing.IsPausedByOwner.Should().BeFalse();

        listing.PauseByOwner();

        listing.IsPausedByOwner.Should().BeTrue();
    }

    [Fact]
    public void ResumeByOwner_sets_IsPausedByOwner_to_false()
    {
        var listing = MakeListing();
        listing.PauseByOwner();
        listing.IsPausedByOwner.Should().BeTrue();

        listing.ResumeByOwner();

        listing.IsPausedByOwner.Should().BeFalse();
    }

    [Fact]
    public void PauseByOwner_does_not_change_Status()
    {
        var listing = MakeListing();
        listing.Approve(Guid.NewGuid());
        listing.Status.Should().Be(ListingStatus.Approved);

        listing.PauseByOwner();

        listing.Status.Should().Be(ListingStatus.Approved);
    }

    [Fact]
    public void ResumeByOwner_does_not_change_Status()
    {
        var listing = MakeListing();
        listing.Approve(Guid.NewGuid());
        listing.PauseByOwner();

        listing.ResumeByOwner();

        listing.Status.Should().Be(ListingStatus.Approved);
    }

    [Fact]
    public void PauseByOwner_updates_UpdatedAt()
    {
        var listing = MakeListing();
        var before = listing.UpdatedAt;

        listing.PauseByOwner();

        listing.UpdatedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void ResumeByOwner_updates_UpdatedAt()
    {
        var listing = MakeListing();
        listing.PauseByOwner();
        var before = listing.UpdatedAt;

        listing.ResumeByOwner();

        listing.UpdatedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void Edit_on_paused_listing_does_not_change_pause_state()
    {
        var listing = MakeListing();
        listing.Approve(Guid.NewGuid());
        listing.PauseByOwner();

        listing.Edit("New EN", "New ES", null, null, null, null);

        // Edit() sets Pending (regla #5) but does NOT touch IsPausedByOwner.
        listing.Status.Should().Be(ListingStatus.Pending);
        listing.IsPausedByOwner.Should().BeTrue();
    }

    private static Listing MakeListing() => Listing.Create(
        providerId: Guid.NewGuid(),
        categoryId: Guid.NewGuid(),
        providerTier: ProviderTier.Regular,
        titleEn: "Test Listing",
        titleEs: "Anuncio de prueba");
}
