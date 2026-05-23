using Bulletin.Board.Application.Commands.Listings;
using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Application.Queries.Listings;
using Bulletin.Board.Application.Snapshots;
using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Bulletin.Board.Application.Tests.Commands.Listings;

public class UpdateOwnListingCommand_WithWhatsAppNumber_PersistsAndSetsPending
{
    private readonly IListingRepository _listings = Substitute.For<IListingRepository>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();
    private readonly IListingSnapshotBuilder _snapshot = Substitute.For<IListingSnapshotBuilder>();
    private readonly UpdateOwnListingCommandHandler _sut;

    private static readonly Guid OwnerUserId = Guid.NewGuid();

    public UpdateOwnListingCommand_WithWhatsAppNumber_PersistsAndSetsPending()
    {
        _sut = new UpdateOwnListingCommandHandler(
            _listings, _user, _snapshot,
            NullLogger<UpdateOwnListingCommandHandler>.Instance);
    }

    [Fact]
    public async Task Edit_persists_whatsapp_number_and_reverts_status_to_pending()
    {
        var listing = BuildApprovedListing();
        listing.Status.Should().Be(ListingStatus.Approved);

        _user.UserId.Returns(OwnerUserId);
        _listings.GetAdminDetailAsync(listing.Id, Arg.Any<CancellationToken>()).Returns(listing);

        // El handler llama GetAdminDetailAsync dos veces: auth y reload.
        // La segunda (reload) devuelve el mismo objeto actualizado.
        _snapshot.Parse(Arg.Any<string?>()).Returns((ListingSnapshot?)null);

        var command = new UpdateOwnListingCommand(
            ListingId: listing.Id,
            TitleEn: "Updated Title",
            TitleEs: "Titulo Actualizado",
            DescriptionEn: "EN desc",
            DescriptionEs: "ES desc",
            Price: 99m,
            Location: "Wisconsin Dells, WI",
            WhatsAppNumber: "+16085551234");

        // Act
        var result = await _sut.Handle(command, default);

        // Assert — regla #5: status reverted to Pending.
        listing.Status.Should().Be(ListingStatus.Pending);
        listing.PublishedAt.Should().BeNull();

        // Assert — WhatsApp number persisted in domain entity.
        listing.WhatsAppNumber.Should().Be("+16085551234");

        // Assert — SaveChangesAsync was called.
        await _listings.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Edit_without_whatsapp_preserves_null()
    {
        var listing = BuildApprovedListing();
        _user.UserId.Returns(OwnerUserId);
        _listings.GetAdminDetailAsync(listing.Id, Arg.Any<CancellationToken>()).Returns(listing);
        _snapshot.Parse(Arg.Any<string?>()).Returns((ListingSnapshot?)null);

        var command = new UpdateOwnListingCommand(
            ListingId: listing.Id,
            TitleEn: "T", TitleEs: "T",
            DescriptionEn: null, DescriptionEs: null,
            Price: null, Location: null,
            WhatsAppNumber: null);

        await _sut.Handle(command, default);

        listing.WhatsAppNumber.Should().BeNull();
        listing.Status.Should().Be(ListingStatus.Pending);
    }

    private Listing BuildApprovedListing()
    {
        var provider = Provider.Create(OwnerUserId, "+15551234567");
        var category = Category.Create("Services", "Servicios", "services", 1, "🔧");

        var listing = Listing.Create(
            providerId: provider.Id,
            categoryId: category.Id,
            providerTier: ProviderTier.Regular,
            titleEn: "Original Title",
            titleEs: "Titulo Original",
            whatsAppNumber: "+15559990000");

        typeof(Listing).GetProperty(nameof(Listing.Provider))!.SetValue(listing, provider);
        typeof(Listing).GetProperty(nameof(Listing.Category))!.SetValue(listing, category);

        listing.Approve(Guid.NewGuid());
        return listing;
    }
}
