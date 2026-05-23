using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Exceptions;
using FluentAssertions;

namespace Bulletin.Board.Domain.Tests.Entities;

public class ReservationMarkCompletedTests
{
    [Fact]
    public void MarkCompleted_from_Accepted_sets_Completed_and_CompletedAt()
    {
        var reservation = MakeReservation();
        reservation.Accept();
        reservation.Status.Should().Be(ReservationStatus.Accepted);

        reservation.MarkCompleted();

        reservation.Status.Should().Be(ReservationStatus.Completed);
        reservation.CompletedAt.Should().NotBeNull();
        reservation.CompletedAt!.Value.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void MarkCompleted_from_Pending_throws_DomainException()
    {
        var reservation = MakeReservation();
        reservation.Status.Should().Be(ReservationStatus.Pending);

        var act = () => reservation.MarkCompleted();

        act.Should().Throw<DomainException>()
            .WithMessage("*Pending*");
    }

    [Fact]
    public void MarkCompleted_from_Rejected_throws_DomainException()
    {
        var reservation = MakeReservation();
        reservation.Reject();

        var act = () => reservation.MarkCompleted();

        act.Should().Throw<DomainException>()
            .WithMessage("*Rejected*");
    }

    [Fact]
    public void MarkCompleted_from_Cancelled_throws_DomainException()
    {
        var reservation = MakeReservation();
        reservation.Cancel();

        var act = () => reservation.MarkCompleted();

        act.Should().Throw<DomainException>()
            .WithMessage("*Cancelled*");
    }

    private static ReservationRequest MakeReservation()
    {
        // Use a future date to avoid domain validation rejecting past start dates.
        var start = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var end = start.AddDays(3);

        return ReservationRequest.Create(
            vehicleId: Guid.NewGuid(),
            listingId: Guid.NewGuid(),
            requesterUserId: Guid.NewGuid(),
            requesterName: "Jane Doe",
            requesterEmail: "jane@example.com",
            requesterPhone: null,
            startDate: start,
            endDate: end,
            comment: null);
    }
}
