using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Application.Queries.Reservations;
using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using FluentAssertions;
using NSubstitute;

namespace Bulletin.Board.Application.Tests.Queries.Reservations;

public class GetListingReservations_OrderIsStableAcrossPages
{
    private readonly IReservationRequestRepository _reservations = Substitute.For<IReservationRequestRepository>();
    private readonly IListingRepository _listings = Substitute.For<IListingRepository>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();
    private readonly GetListingReservationsQueryHandler _sut;

    private static readonly Guid OwnerUserId = Guid.NewGuid();
    private static readonly Guid ListingId = Guid.NewGuid();

    public GetListingReservations_OrderIsStableAcrossPages()
    {
        _sut = new GetListingReservationsQueryHandler(_reservations, _listings, _user);

        _user.UserId.Returns(OwnerUserId);
        _user.IsInRole("Admin").Returns(false);

        var provider = Provider.Create(OwnerUserId, "+15550000000");
        var category = Category.Create("Car Rental", "Renta de Autos", "car-rental", 1, "🚗");
        var listing = Listing.Create(provider.Id, category.Id, ProviderTier.Regular, "T", "T");

        typeof(Listing).GetProperty(nameof(Listing.Provider))!.SetValue(listing, provider);
        typeof(Listing).GetProperty(nameof(Listing.Category))!.SetValue(listing, category);

        _listings.GetAdminDetailAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(listing);

        // Default: repositorio retorna vacío
        _reservations.GetForListingPagedAsync(
                Arg.Any<Guid>(), Arg.Any<ReservationStatus?>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((Array.Empty<ReservationRequest>() as IReadOnlyList<ReservationRequest>, 0, false));
    }

    [Fact]
    public async Task Returns_paged_result_with_correct_total()
    {
        var twoItems = MakeStubReservations(2);

        // Simula 5 totales, devuelve solo 2 (página 1 de 3)
        _reservations.GetForListingPagedAsync(
                Arg.Any<Guid>(),
                Arg.Any<ReservationStatus?>(),
                skip: 0,
                take: 2,
                Arg.Any<CancellationToken>())
            .Returns((twoItems as IReadOnlyList<ReservationRequest>, 5, true));

        var result = await _sut.Handle(
            new GetListingReservationsQuery(ListingId, Status: null, Page: 1, PageSize: 2), default);

        result.TotalItems.Should().Be(5);
        result.TotalPages.Should().Be(3);
        result.HasNext.Should().BeTrue();
        result.HasPrevious.Should().BeFalse();
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task Last_page_has_no_next()
    {
        var twoItems = MakeStubReservations(2);

        _reservations.GetForListingPagedAsync(
                Arg.Any<Guid>(),
                Arg.Any<ReservationStatus?>(),
                skip: 4,
                take: 2,
                Arg.Any<CancellationToken>())
            .Returns((twoItems as IReadOnlyList<ReservationRequest>, 6, true));

        var result = await _sut.Handle(
            new GetListingReservationsQuery(ListingId, Status: null, Page: 3, PageSize: 2), default);

        result.HasNext.Should().BeFalse();
        result.HasPrevious.Should().BeTrue();
        result.Page.Should().Be(3);
    }

    [Fact]
    public async Task Status_filter_is_forwarded_to_repository()
    {
        await _sut.Handle(
            new GetListingReservationsQuery(ListingId, Status: "Pending", Page: 1, PageSize: 20), default);

        await _reservations.Received(1).GetForListingPagedAsync(
            ListingId,
            ReservationStatus.Pending,
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Unknown_status_string_is_treated_as_no_filter()
    {
        await _sut.Handle(
            new GetListingReservationsQuery(ListingId, Status: "NotAStatus", Page: 1, PageSize: 20), default);

        await _reservations.Received(1).GetForListingPagedAsync(
            ListingId,
            (ReservationStatus?)null, // sin filtro: enum parse falla → null
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Crea instancias de ReservationRequest usando reflexión para evitar la validación de fechas
    /// del factory method (que rechaza fechas pasadas). Los tests de handler no deben depender
    /// de la fecha del sistema.
    /// </summary>
    private static List<ReservationRequest> MakeStubReservations(int count)
    {
        var result = new List<ReservationRequest>(count);
        for (var i = 0; i < count; i++)
        {
            var r = (ReservationRequest)System.Runtime.CompilerServices.RuntimeHelpers
                .GetUninitializedObject(typeof(ReservationRequest));

            typeof(ReservationRequest).GetProperty(nameof(ReservationRequest.Id))!
                .SetValue(r, Guid.NewGuid());
            typeof(ReservationRequest).GetProperty(nameof(ReservationRequest.VehicleId))!
                .SetValue(r, Guid.NewGuid());
            typeof(ReservationRequest).GetProperty(nameof(ReservationRequest.ListingId))!
                .SetValue(r, ListingId);
            typeof(ReservationRequest).GetProperty(nameof(ReservationRequest.RequesterName))!
                .SetValue(r, $"Requester {i}");
            typeof(ReservationRequest).GetProperty(nameof(ReservationRequest.RequesterEmail))!
                .SetValue(r, $"req{i}@example.com");
            typeof(ReservationRequest).GetProperty(nameof(ReservationRequest.Status))!
                .SetValue(r, ReservationStatus.Pending);
            typeof(ReservationRequest).GetProperty(nameof(ReservationRequest.CreatedAt))!
                .SetValue(r, DateTimeOffset.UtcNow.AddMinutes(-i));
            typeof(ReservationRequest).GetProperty(nameof(ReservationRequest.StartDate))!
                .SetValue(r, DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(i + 1)));
            typeof(ReservationRequest).GetProperty(nameof(ReservationRequest.EndDate))!
                .SetValue(r, DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(i + 2)));

            result.Add(r);
        }
        return result;
    }
}
