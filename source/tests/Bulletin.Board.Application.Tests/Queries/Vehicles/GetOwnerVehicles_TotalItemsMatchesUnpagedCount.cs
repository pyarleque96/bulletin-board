using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Application.Queries.Vehicles;
using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using FluentAssertions;
using NSubstitute;

namespace Bulletin.Board.Application.Tests.Queries.Vehicles;

public class GetOwnerVehicles_TotalItemsMatchesUnpagedCount
{
    private readonly IListingRepository _listings = Substitute.For<IListingRepository>();
    private readonly IVehicleRepository _vehicles = Substitute.For<IVehicleRepository>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();
    private readonly GetOwnerVehiclesQueryHandler _sut;

    private static readonly Guid OwnerUserId = Guid.NewGuid();
    private static readonly Guid ListingId = Guid.NewGuid();

    public GetOwnerVehicles_TotalItemsMatchesUnpagedCount()
    {
        _sut = new GetOwnerVehiclesQueryHandler(_listings, _vehicles, _user);

        _user.UserId.Returns(OwnerUserId);
        _user.IsInRole("Admin").Returns(false);

        var provider = Provider.Create(OwnerUserId, "+15550000000");
        var category = Category.Create("Car Rental", "Renta de Autos", "car-rental", 1, "🚗");
        var listing = Listing.Create(provider.Id, category.Id, ProviderTier.Regular, "Rental", "Renta");

        typeof(Listing).GetProperty(nameof(Listing.Provider))!.SetValue(listing, provider);
        typeof(Listing).GetProperty(nameof(Listing.Category))!.SetValue(listing, category);

        _listings.GetAdminDetailAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(listing);

        // Default: vacío
        _vehicles.GetByListingForOwnerPagedAsync(
                Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((Array.Empty<Vehicle>() as IReadOnlyList<Vehicle>, 0));
    }

    [Fact]
    public async Task TotalItems_matches_repository_total_not_page_items_count()
    {
        var pageVehicles = MakeVehicles(3);

        // 10 vehículos en total, devolvemos solo los primeros 3
        _vehicles.GetByListingForOwnerPagedAsync(
                ListingId,
                skip: 0,
                take: 3,
                Arg.Any<CancellationToken>())
            .Returns((pageVehicles as IReadOnlyList<Vehicle>, 10));

        var result = await _sut.Handle(
            new GetOwnerVehiclesQuery(ListingId, Page: 1, PageSize: 3), default);

        result.TotalItems.Should().Be(10, "el total debe reflejar el count del repositorio, no el count de la página");
        result.Items.Should().HaveCount(3);
        result.TotalPages.Should().Be(4); // ceil(10/3) = 4
        result.HasNext.Should().BeTrue();
    }

    [Fact]
    public async Task Empty_listing_returns_zero_total_and_no_navigation()
    {
        _vehicles.GetByListingForOwnerPagedAsync(
                ListingId,
                skip: 0,
                take: 20,
                Arg.Any<CancellationToken>())
            .Returns((Array.Empty<Vehicle>() as IReadOnlyList<Vehicle>, 0));

        var result = await _sut.Handle(
            new GetOwnerVehiclesQuery(ListingId, Page: 1, PageSize: 20), default);

        result.TotalItems.Should().Be(0);
        result.Items.Should().BeEmpty();
        result.TotalPages.Should().Be(0);
        result.HasNext.Should().BeFalse();
        result.HasPrevious.Should().BeFalse();
    }

    [Fact]
    public async Task PageSize_is_clamped_to_100_before_forwarding_to_repository()
    {
        await _sut.Handle(
            new GetOwnerVehiclesQuery(ListingId, Page: 1, PageSize: 500), default);

        await _vehicles.Received(1).GetByListingForOwnerPagedAsync(
            ListingId,
            skip: 0,
            take: 100,
            Arg.Any<CancellationToken>());
    }

    private static List<Vehicle> MakeVehicles(int count)
    {
        var result = new List<Vehicle>(count);
        for (var i = 0; i < count; i++)
        {
            var v = Vehicle.Create(
                listingId: ListingId,
                name: $"Vehicle {i}",
                passengerMax: 4,
                transmission: Transmission.Automatic,
                hasAirConditioning: true,
                dailyRateCents: 3000 + (i * 100),
                weeklyRateCents: null,
                monthlyRateCents: null,
                displayOrder: i);
            result.Add(v);
        }
        return result;
    }
}
