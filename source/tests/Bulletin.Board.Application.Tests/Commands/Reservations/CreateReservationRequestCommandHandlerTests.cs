using Bulletin.Board.Application.Commands.Reservations;
using Bulletin.Board.Application.Settings;
using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Bulletin.Board.Application.Tests.Commands.Reservations;

public class CreateReservationRequestCommandHandlerTests
{
    private readonly IListingRepository _listings = Substitute.For<IListingRepository>();
    private readonly IVehicleRepository _vehicles = Substitute.For<IVehicleRepository>();
    private readonly IReservationRequestRepository _reservations = Substitute.For<IReservationRequestRepository>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();
    private readonly IEmailService _email = Substitute.For<IEmailService>();
    private readonly CreateReservationRequestCommandHandler _sut;
    private readonly Guid _userId = Guid.NewGuid();

    public CreateReservationRequestCommandHandlerTests()
    {
        _sut = new CreateReservationRequestCommandHandler(
            _listings, _vehicles, _reservations, _user, _email,
            Options.Create(new NotificationSettings { GmEmail = "gm@test.local" }),
            NullLogger<CreateReservationRequestCommandHandler>.Instance);
    }

    [Fact]
    public async Task Throws_when_user_not_authenticated()
    {
        _user.UserId.Returns((Guid?)null);
        var act = () => _sut.Handle(MakeCommand(Guid.NewGuid()), default);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Throws_when_vehicle_not_found()
    {
        _user.UserId.Returns(_userId);
        _vehicles.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Vehicle?)null);

        var act = () => _sut.Handle(MakeCommand(Guid.NewGuid()), default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Vehicle*");
    }

    [Fact]
    public async Task Throws_when_vehicle_inactive()
    {
        _user.UserId.Returns(_userId);
        var vehicle = MakeVehicle();
        vehicle.Deactivate();
        _vehicles.GetByIdAsync(vehicle.Id, Arg.Any<CancellationToken>()).Returns(vehicle);

        var act = () => _sut.Handle(MakeCommand(vehicle.Id), default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*not currently accepting*");
    }

    [Fact]
    public async Task Throws_when_listing_not_approved()
    {
        _user.UserId.Returns(_userId);
        var vehicle = MakeVehicle();
        var listing = MakeListing(approved: false);
        _vehicles.GetByIdAsync(vehicle.Id, Arg.Any<CancellationToken>()).Returns(vehicle);
        _listings.GetByIdAsync(vehicle.ListingId, Arg.Any<CancellationToken>()).Returns(listing);

        var act = () => _sut.Handle(MakeCommand(vehicle.Id), default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*not currently available*");
    }

    [Fact]
    public async Task Throws_when_dates_overlap_existing_blackout()
    {
        _user.UserId.Returns(_userId);
        var vehicle = MakeVehicle();
        var listing = MakeListing(approved: true);
        _vehicles.GetByIdAsync(vehicle.Id, Arg.Any<CancellationToken>()).Returns(vehicle);
        _listings.GetByIdAsync(vehicle.ListingId, Arg.Any<CancellationToken>()).Returns(listing);
        _vehicles.GetUnavailabilityInRangeAsync(
            Arg.Any<Guid>(), Arg.Any<DateOnly>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(new List<VehicleUnavailability>
            {
                VehicleUnavailability.Create(vehicle.Id,
                    new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 5),
                    UnavailabilityReason.Manual)
            });

        var act = () => _sut.Handle(MakeCommand(vehicle.Id), default);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*conflict*");
    }

    [Fact]
    public async Task Creates_reservation_in_pending_state_and_notifies_gm()
    {
        _user.UserId.Returns(_userId);
        var vehicle = MakeVehicle();
        var listing = MakeListing(approved: true);
        _vehicles.GetByIdAsync(vehicle.Id, Arg.Any<CancellationToken>()).Returns(vehicle);
        _listings.GetByIdAsync(vehicle.ListingId, Arg.Any<CancellationToken>()).Returns(listing);
        _vehicles.GetUnavailabilityInRangeAsync(
            Arg.Any<Guid>(), Arg.Any<DateOnly>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(new List<VehicleUnavailability>());

        var id = await _sut.Handle(MakeCommand(vehicle.Id), default);

        id.Should().NotBeEmpty();
        await _reservations.Received(1).AddAsync(Arg.Any<ReservationRequest>(), Arg.Any<CancellationToken>());
        await _reservations.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _email.Received(1).SendAsync(
            "gm@test.local",
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Email_failure_does_not_break_creation()
    {
        _user.UserId.Returns(_userId);
        var vehicle = MakeVehicle();
        var listing = MakeListing(approved: true);
        _vehicles.GetByIdAsync(vehicle.Id, Arg.Any<CancellationToken>()).Returns(vehicle);
        _listings.GetByIdAsync(vehicle.ListingId, Arg.Any<CancellationToken>()).Returns(listing);
        _vehicles.GetUnavailabilityInRangeAsync(
            Arg.Any<Guid>(), Arg.Any<DateOnly>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(new List<VehicleUnavailability>());
        _email.SendAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromException(new Exception("SMTP down")));

        var id = await _sut.Handle(MakeCommand(vehicle.Id), default);
        id.Should().NotBeEmpty();
    }

    private static Vehicle MakeVehicle() => Vehicle.Create(
        listingId: Guid.NewGuid(),
        name: "Test Car",
        passengerMax: 4,
        transmission: Transmission.Automatic,
        hasAirConditioning: true,
        dailyRateCents: 3900);

    private static Listing MakeListing(bool approved)
    {
        var listing = Listing.Create(
            providerId: Guid.NewGuid(),
            categoryId: Guid.NewGuid(),
            providerTier: ProviderTier.VIP,
            titleEn: "T", titleEs: "T");
        if (approved) listing.Approve(Guid.NewGuid());
        return listing;
    }

    private static CreateReservationRequestCommand MakeCommand(Guid vehicleId) => new(
        VehicleId: vehicleId,
        StartDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
        EndDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
        RequesterName: "Jane Doe",
        RequesterEmail: "jane@example.com",
        RequesterPhone: "+15551234567",
        Comment: "Need it for a weekend trip.");
}
