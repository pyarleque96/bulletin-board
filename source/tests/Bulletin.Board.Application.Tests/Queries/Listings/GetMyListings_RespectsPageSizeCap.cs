using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Application.Queries.Listings;
using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using FluentAssertions;
using NSubstitute;

namespace Bulletin.Board.Application.Tests.Queries.Listings;

public class GetMyListings_RespectsPageSizeCap
{
    private readonly IListingRepository _listings = Substitute.For<IListingRepository>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();
    private readonly GetMyListingsQueryHandler _sut;

    private static readonly Guid OwnerUserId = Guid.NewGuid();

    public GetMyListings_RespectsPageSizeCap()
    {
        _sut = new GetMyListingsQueryHandler(_listings, _user);
        _user.UserId.Returns(OwnerUserId);

        // Por defecto retorna lista vacía; cada test sobreescribe si necesita datos.
        _listings.GetByProviderUserIdPagedAsync(
                Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((Array.Empty<Listing>() as IReadOnlyList<Listing>, 0));
    }

    [Theory]
    [InlineData(0, 20)]    // pageSize=0 → normaliza a 20
    [InlineData(-5, 20)]   // negativo → normaliza a 20
    [InlineData(101, 100)] // supera cap → clampea a 100
    [InlineData(9999, 100)] // muy grande → clampea a 100
    [InlineData(50, 50)]   // válido → pasa tal cual
    public async Task PagedRequest_normalizes_page_size(int requestedSize, int expectedTake)
    {
        await _sut.Handle(new GetMyListingsQuery(Page: 1, PageSize: requestedSize), default);

        // Verifica que el repositorio recibió el Take correcto (el clampado), no el valor crudo.
        await _listings.Received(1).GetByProviderUserIdPagedAsync(
            OwnerUserId,
            Arg.Any<int>(),        // skip
            expectedTake,          // take — debe ser el valor normalizado
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(0, 1)]   // page=0 → normaliza a 1
    [InlineData(-1, 1)]  // page negativa → normaliza a 1
    [InlineData(1, 1)]   // válido
    [InlineData(5, 5)]   // válido
    public async Task PagedRequest_normalizes_page_number(int requestedPage, int expectedSkipPage)
    {
        await _sut.Handle(new GetMyListingsQuery(Page: requestedPage, PageSize: 10), default);

        var expectedSkip = (expectedSkipPage - 1) * 10;

        await _listings.Received(1).GetByProviderUserIdPagedAsync(
            OwnerUserId,
            expectedSkip,
            10,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Result_pageSize_reflects_clamped_take_not_raw_request()
    {
        var result = await _sut.Handle(new GetMyListingsQuery(Page: 1, PageSize: 9999), default);

        // El resultado expone el Take real (100), no el 9999 del cliente.
        result.PageSize.Should().Be(100);
    }
}
