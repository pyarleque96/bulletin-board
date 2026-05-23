using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Application.Queries.Listings;
using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using FluentAssertions;
using NSubstitute;

namespace Bulletin.Board.Application.Tests.Queries.Listings;

public class GetMyListings_ReturnsExtendedFields
{
    private readonly IListingRepository _listings = Substitute.For<IListingRepository>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();
    private readonly GetMyListingsQueryHandler _sut;

    private static readonly Guid OwnerUserId = Guid.NewGuid();

    public GetMyListings_ReturnsExtendedFields()
    {
        _sut = new GetMyListingsQueryHandler(_listings, _user);
    }

    [Fact]
    public async Task Returns_description_price_location_and_whatsapp_in_dto()
    {
        _user.UserId.Returns(OwnerUserId);

        var listing = BuildListing(
            descEn: "EN description",
            descEs: "ES descripcion",
            price: 49.99m,
            location: "Wisconsin Dells, WI",
            whatsApp: "+16085550000");

        _listings.GetByProviderUserIdPagedAsync(
                OwnerUserId,
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns((new[] { listing }.ToList() as IReadOnlyList<Listing>, 1));

        var result = await _sut.Handle(new GetMyListingsQuery(Page: 1, PageSize: 20), default);

        result.Items.Should().HaveCount(1);
        var dto = result.Items[0];
        dto.DescriptionEn.Should().Be("EN description");
        dto.DescriptionEs.Should().Be("ES descripcion");
        dto.Price.Should().Be(49.99m);
        dto.Location.Should().Be("Wisconsin Dells, WI");
        dto.WhatsAppNumber.Should().Be("+16085550000");
    }

    [Fact]
    public async Task Returns_correct_paging_metadata()
    {
        _user.UserId.Returns(OwnerUserId);

        var listing = BuildListing();

        // Simula 3 items totales pero la página devuelve 1
        _listings.GetByProviderUserIdPagedAsync(
                OwnerUserId,
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns((new[] { listing }.ToList() as IReadOnlyList<Listing>, 3));

        var result = await _sut.Handle(new GetMyListingsQuery(Page: 1, PageSize: 1), default);

        result.TotalItems.Should().Be(3);
        result.TotalPages.Should().Be(3);
        result.HasNext.Should().BeTrue();
        result.HasPrevious.Should().BeFalse();
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(1);
    }

    [Fact]
    public async Task Throws_when_user_not_authenticated()
    {
        _user.UserId.Returns((Guid?)null);

        var act = () => _sut.Handle(new GetMyListingsQuery(), default);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    private static Listing BuildListing(
        string? descEn = null,
        string? descEs = null,
        decimal? price = null,
        string? location = null,
        string? whatsApp = null)
    {
        var provider = Provider.Create(OwnerUserId, "+15550000000");
        var category = Category.Create("Housing", "Vivienda", "housing", 1, "🏠");

        var listing = Listing.Create(
            providerId: provider.Id,
            categoryId: category.Id,
            providerTier: ProviderTier.Regular,
            titleEn: "Test Listing",
            titleEs: "Anuncio de Prueba",
            descriptionEn: descEn,
            descriptionEs: descEs,
            price: price,
            location: location,
            whatsAppNumber: whatsApp);

        typeof(Listing).GetProperty(nameof(Listing.Category))!.SetValue(listing, category);
        typeof(Listing).GetProperty(nameof(Listing.Provider))!.SetValue(listing, provider);

        return listing;
    }
}
