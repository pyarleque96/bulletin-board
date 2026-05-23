using System.Text.Json;
using Bulletin.Board.Application.Snapshots;
using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using FluentAssertions;

namespace Bulletin.Board.Application.Tests.Snapshots;

public class ListingSnapshotBuilderTests
{
    private readonly IListingSnapshotBuilder _sut;

    public ListingSnapshotBuilderTests()
    {
        _sut = new ListingSnapshotBuilder();  // visible via InternalsVisibleTo
    }

    [Fact]
    public void Build_serializes_minimal_listing_with_no_children()
    {
        var listing = MakeListing();

        var json = _sut.Build(listing);
        var parsed = _sut.Parse(json);

        parsed.Should().NotBeNull();
        parsed!.Version.Should().Be(ListingSnapshot.CurrentVersion);
        parsed.TitleEn.Should().Be(listing.TitleEn);
        parsed.TitleEs.Should().Be(listing.TitleEs);
        parsed.Images.Should().BeEmpty();
        parsed.Vehicles.Should().BeEmpty();
    }

    [Fact]
    public void Build_includes_only_public_images_in_display_order()
    {
        var listing = MakeListing();
        var pubA = ListingImage.Create(listing.Id, "/a.jpg", displayOrder: 2);
        var pubB = ListingImage.Create(listing.Id, "/b.jpg", displayOrder: 1);
        var priv = ListingImage.Create(listing.Id, "/c.jpg", displayOrder: 0);
        pubA.MakePublic();
        pubB.MakePublic();
        // priv left non-public
        listing.Images.Add(pubA);
        listing.Images.Add(pubB);
        listing.Images.Add(priv);

        var parsed = _sut.Parse(_sut.Build(listing))!;

        parsed.Images.Should().HaveCount(2);
        parsed.Images[0].FilePath.Should().Be("/b.jpg");  // displayOrder 1
        parsed.Images[1].FilePath.Should().Be("/a.jpg");  // displayOrder 2
    }

    [Fact]
    public void Build_excludes_inactive_vehicles()
    {
        var listing = MakeListing();
        var active = MakeVehicle(listing.Id, "Active");
        var inactive = MakeVehicle(listing.Id, "Inactive");
        inactive.Deactivate();
        listing.Vehicles.Add(active);
        listing.Vehicles.Add(inactive);

        var parsed = _sut.Parse(_sut.Build(listing))!;

        parsed.Vehicles.Should().HaveCount(1);
        parsed.Vehicles[0].Name.Should().Be("Active");
    }

    [Fact]
    public void Build_includes_only_public_vehicle_photos_with_primary_first()
    {
        var listing = MakeListing();
        var v = MakeVehicle(listing.Id, "Car");
        var p1 = VehiclePhoto.Create(v.Id, "/p1.jpg", displayOrder: 0);
        var p2 = VehiclePhoto.Create(v.Id, "/p2.jpg", displayOrder: 1);
        var p3 = VehiclePhoto.Create(v.Id, "/p3.jpg", displayOrder: 2);
        p1.MakePublic();
        p2.MakePublic();
        p3.MakePublic();
        p2.MarkPrimary();
        // Add a private one — must be excluded
        var priv = VehiclePhoto.Create(v.Id, "/secret.jpg", displayOrder: 99);
        v.Photos.Add(p1);
        v.Photos.Add(p2);
        v.Photos.Add(p3);
        v.Photos.Add(priv);
        listing.Vehicles.Add(v);

        var parsed = _sut.Parse(_sut.Build(listing))!;

        parsed.Vehicles.Should().HaveCount(1);
        parsed.Vehicles[0].Photos.Should().HaveCount(3);
        parsed.Vehicles[0].Photos[0].FilePath.Should().Be("/p2.jpg");  // primary first
        parsed.Vehicles[0].Photos[0].IsPrimary.Should().BeTrue();
    }

    [Fact]
    public void Build_uses_camel_case_keys_in_json()
    {
        var listing = MakeListing();
        var json = _sut.Build(listing);

        // PG-friendly keys for direct querying via psql/pgAdmin.
        json.Should().Contain("\"titleEn\":");
        json.Should().Contain("\"version\":");
        json.Should().NotContain("\"TitleEn\":");
    }

    [Fact]
    public void Parse_returns_null_for_empty_or_invalid_json()
    {
        _sut.Parse(null).Should().BeNull();
        _sut.Parse("").Should().BeNull();
        _sut.Parse("   ").Should().BeNull();
        _sut.Parse("{ malformed").Should().BeNull();
    }

    [Fact]
    public void Build_then_Parse_roundtrips_all_fields()
    {
        var listing = MakeListing();
        listing.Images.Add(MakePublicImage(listing.Id, "/a.jpg", 0));
        var v = MakeVehicle(listing.Id, "Compact");
        var p = VehiclePhoto.Create(v.Id, "/v1.jpg", 0);
        p.MakePublic();
        v.Photos.Add(p);
        listing.Vehicles.Add(v);

        var roundtripped = _sut.Parse(_sut.Build(listing))!;

        roundtripped.TitleEn.Should().Be(listing.TitleEn);
        roundtripped.Price.Should().Be(listing.Price);
        roundtripped.Images.Single().FilePath.Should().Be("/a.jpg");
        roundtripped.Vehicles.Single().Name.Should().Be("Compact");
        roundtripped.Vehicles.Single().Photos.Single().FilePath.Should().Be("/v1.jpg");
        roundtripped.Vehicles.Single().Transmission.Should().Be("Automatic");
    }

    private static Listing MakeListing()
    {
        return Listing.Create(
            providerId: Guid.NewGuid(),
            categoryId: Guid.NewGuid(),
            providerTier: ProviderTier.VIP,
            titleEn: "Dells Premier",
            titleEs: "Dells Premier",
            descriptionEn: "Desc",
            price: 39m,
            location: "Wisconsin Dells");
    }

    private static Vehicle MakeVehicle(Guid listingId, string name) => Vehicle.Create(
        listingId: listingId,
        name: name,
        passengerMax: 4,
        transmission: Transmission.Automatic,
        hasAirConditioning: true,
        dailyRateCents: 3900);

    private static ListingImage MakePublicImage(Guid listingId, string path, int order)
    {
        var img = ListingImage.Create(listingId, path, order);
        img.MakePublic();
        return img;
    }
}
