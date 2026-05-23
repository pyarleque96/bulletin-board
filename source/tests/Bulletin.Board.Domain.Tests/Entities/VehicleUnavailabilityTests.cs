using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Exceptions;
using FluentAssertions;

namespace Bulletin.Board.Domain.Tests.Entities;

public class VehicleUnavailabilityTests
{
    private static readonly DateOnly Day1 = new(2026, 6, 1);
    private static readonly DateOnly Day5 = new(2026, 6, 5);

    [Fact]
    public void Create_with_manual_reason_works_without_reservation()
    {
        var u = VehicleUnavailability.Create(
            Guid.NewGuid(), Day1, Day5, UnavailabilityReason.Manual);
        u.Reason.Should().Be(UnavailabilityReason.Manual);
        u.ReservationId.Should().BeNull();
    }

    [Fact]
    public void Create_with_reserved_reason_requires_reservation_id()
    {
        var act = () => VehicleUnavailability.Create(
            Guid.NewGuid(), Day1, Day5, UnavailabilityReason.Reserved);
        act.Should().Throw<DomainException>().WithMessage("*reservation*");
    }

    [Fact]
    public void Create_with_non_reserved_reason_rejects_reservation_id()
    {
        var act = () => VehicleUnavailability.Create(
            Guid.NewGuid(), Day1, Day5, UnavailabilityReason.Maintenance, Guid.NewGuid());
        act.Should().Throw<DomainException>().WithMessage("*Reserved*");
    }

    [Fact]
    public void Create_rejects_end_before_start()
    {
        var act = () => VehicleUnavailability.Create(
            Guid.NewGuid(), Day5, Day1, UnavailabilityReason.Manual);
        act.Should().Throw<DomainException>().WithMessage("*before*");
    }

    [Fact]
    public void Create_accepts_single_day_blackout()
    {
        var u = VehicleUnavailability.Create(
            Guid.NewGuid(), Day1, Day1, UnavailabilityReason.Manual);
        u.StartDate.Should().Be(Day1);
        u.EndDate.Should().Be(Day1);
    }

    [Theory]
    [InlineData(2026, 5, 31, false)]  // before
    [InlineData(2026, 6, 1, true)]    // start (inclusive)
    [InlineData(2026, 6, 3, true)]    // middle
    [InlineData(2026, 6, 5, true)]    // end (inclusive)
    [InlineData(2026, 6, 6, false)]   // after
    public void Covers_returns_true_for_dates_inside_inclusive_range(int y, int m, int d, bool expected)
    {
        var u = VehicleUnavailability.Create(
            Guid.NewGuid(), Day1, Day5, UnavailabilityReason.Manual);
        u.Covers(new DateOnly(y, m, d)).Should().Be(expected);
    }
}
