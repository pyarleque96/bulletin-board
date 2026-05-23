using Bulletin.Board.Application.Commands.Vehicles;
using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Bulletin.Board.Application.Tests.Commands.Vehicles;

public class CreateVehicleCommandHandlerTests
{
    private readonly IListingRepository _listings = Substitute.For<IListingRepository>();
    private readonly IVehicleRepository _vehicles = Substitute.For<IVehicleRepository>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();
    private readonly CreateVehicleCommandHandler _sut;

    private static readonly Guid OwnerUserId = Guid.NewGuid();
    private static readonly Guid OtherUserId = Guid.NewGuid();

    public CreateVehicleCommandHandlerTests()
    {
        _sut = new CreateVehicleCommandHandler(
            _listings, _vehicles, _user,
            NullLogger<CreateVehicleCommandHandler>.Instance);
    }

    [Fact]
    public async Task Throws_when_user_not_authenticated()
    {
        _user.UserId.Returns((Guid?)null);

        var act = () => _sut.Handle(MakeCommand(Guid.NewGuid()), default);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Throws_when_listing_not_found()
    {
        _user.UserId.Returns(OwnerUserId);
        _listings.GetAdminDetailAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Listing?)null);

        var act = () => _sut.Handle(MakeCommand(Guid.NewGuid()), default);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*not found*");
    }

    [Fact]
    public async Task Throws_when_user_is_not_owner_and_not_admin()
    {
        var listing = SetUpListing("car-rental");
        _user.UserId.Returns(OtherUserId);
        _user.IsInRole("Admin").Returns(false);

        var act = () => _sut.Handle(MakeCommand(listing.Id), default);

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("*owner*");
    }

    [Fact]
    public async Task Throws_when_listing_category_is_not_car_rental()
    {
        var listing = SetUpListing("housing");
        _user.UserId.Returns(OwnerUserId);

        var act = () => _sut.Handle(MakeCommand(listing.Id), default);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*car-rental*");
    }

    [Fact]
    public async Task Throws_when_transmission_string_is_invalid()
    {
        var listing = SetUpListing("car-rental");
        _user.UserId.Returns(OwnerUserId);

        var act = () => _sut.Handle(
            MakeCommand(listing.Id) with { Transmission = "Hybrid" },
            default);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Hybrid*");
    }

    [Fact]
    public async Task Creates_vehicle_and_reverts_listing_to_pending()
    {
        var listing = SetUpListing("car-rental");
        listing.Approve(Guid.NewGuid());          // start as Approved so we can verify the flip
        listing.Status.Should().Be(ListingStatus.Approved);
        _user.UserId.Returns(OwnerUserId);

        var id = await _sut.Handle(MakeCommand(listing.Id), default);

        id.Should().NotBeEmpty();
        listing.Status.Should().Be(ListingStatus.Pending);
        await _vehicles.Received(1).AddAsync(Arg.Any<Vehicle>(), Arg.Any<CancellationToken>());
        await _vehicles.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Admin_can_create_vehicle_for_listing_they_do_not_own()
    {
        var listing = SetUpListing("car-rental");
        _user.UserId.Returns(OtherUserId);
        _user.IsInRole("Admin").Returns(true);

        var id = await _sut.Handle(MakeCommand(listing.Id), default);

        id.Should().NotBeEmpty();
    }

    private Listing SetUpListing(string categorySlug)
    {
        var provider = Provider.Create(OwnerUserId, "+15551234567");
        var category = Category.Create("Car Rental", "Renta", categorySlug, 1, "🚗");

        var listing = Listing.Create(
            providerId: provider.Id,
            categoryId: category.Id,
            providerTier: ProviderTier.VIP,
            titleEn: "Test rental",
            titleEs: "Renta prueba");

        // Wire navigations the handler reads
        typeof(Listing).GetProperty(nameof(Listing.Provider))!
            .SetValue(listing, provider);
        typeof(Listing).GetProperty(nameof(Listing.Category))!
            .SetValue(listing, category);

        _listings.GetAdminDetailAsync(listing.Id, Arg.Any<CancellationToken>()).Returns(listing);
        return listing;
    }

    private static CreateVehicleCommand MakeCommand(Guid listingId) => new(
        ListingId: listingId,
        Name: "Economy Compact",
        PassengerMax: 4,
        Transmission: "Automatic",
        HasAirConditioning: true,
        DailyRateCents: 3900,
        WeeklyRateCents: 21900,
        MonthlyRateCents: 59900,
        DisplayOrder: 0);
}
