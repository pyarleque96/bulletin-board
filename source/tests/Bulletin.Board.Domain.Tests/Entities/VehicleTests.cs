using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Exceptions;
using FluentAssertions;

namespace Bulletin.Board.Domain.Tests.Entities;

public class VehicleTests
{
    [Fact]
    public void Create_persists_normalized_fields()
    {
        var listingId = Guid.NewGuid();
        var v = Vehicle.Create(
            listingId: listingId,
            name: "  Economy Compact  ",
            passengerMax: 4,
            transmission: Transmission.Both,
            hasAirConditioning: true,
            dailyRateCents: 3900,
            weeklyRateCents: 21900,
            monthlyRateCents: 59900,
            displayOrder: 1);

        v.ListingId.Should().Be(listingId);
        v.Name.Should().Be("Economy Compact");
        v.PassengerMax.Should().Be(4);
        v.Transmission.Should().Be(Transmission.Both);
        v.HasAirConditioning.Should().BeTrue();
        v.DailyRateCents.Should().Be(3900);
        v.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_rejects_empty_name(string? name)
    {
        var act = () => Vehicle.Create(
            Guid.NewGuid(), name!, 4, Transmission.Automatic, true, 3900);
        act.Should().Throw<DomainException>().WithMessage("*name*");
    }

    [Fact]
    public void Create_rejects_name_over_80_chars()
    {
        var act = () => Vehicle.Create(
            Guid.NewGuid(), new string('A', 81), 4, Transmission.Automatic, true, 3900);
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(51)]  // exceeds reasonable cap
    public void Create_rejects_invalid_passenger_max(int max)
    {
        var act = () => Vehicle.Create(
            Guid.NewGuid(), "X", max, Transmission.Automatic, true, 3900);
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_rejects_non_positive_daily_rate(int rate)
    {
        var act = () => Vehicle.Create(
            Guid.NewGuid(), "X", 4, Transmission.Automatic, true, rate);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_accepts_null_optional_rates()
    {
        var v = Vehicle.Create(
            Guid.NewGuid(), "X", 4, Transmission.Automatic, true,
            dailyRateCents: 3900, weeklyRateCents: null, monthlyRateCents: null);
        v.WeeklyRateCents.Should().BeNull();
        v.MonthlyRateCents.Should().BeNull();
    }

    [Fact]
    public void Edit_updates_fields_and_bumps_updated_at()
    {
        var v = Vehicle.Create(
            Guid.NewGuid(), "Old", 4, Transmission.Manual, false, 3000);
        var before = v.UpdatedAt;
        Thread.Sleep(5);

        v.Edit("New", 5, Transmission.Automatic, true, 4000, 22000, 60000, 3);

        v.Name.Should().Be("New");
        v.Transmission.Should().Be(Transmission.Automatic);
        v.UpdatedAt.Should().BeAfter(before);
    }

    [Fact]
    public void Deactivate_then_reactivate_toggles_is_active()
    {
        var v = Vehicle.Create(Guid.NewGuid(), "X", 4, Transmission.Automatic, true, 3900);
        v.IsActive.Should().BeTrue();
        v.Deactivate();
        v.IsActive.Should().BeFalse();
        v.Reactivate();
        v.IsActive.Should().BeTrue();
    }
}
