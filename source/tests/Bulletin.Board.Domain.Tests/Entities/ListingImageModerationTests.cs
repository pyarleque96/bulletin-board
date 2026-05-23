using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Exceptions;
using FluentAssertions;

namespace Bulletin.Board.Domain.Tests.Entities;

/// <summary>
/// Tests for regla #4: el proveedor sube fotos pero el GM decide cuáles son públicas.
/// El default al crearse es Pending; Public/Hidden/Rejected solo via Moderate.
/// </summary>
public class ListingImageModerationTests
{
    [Fact]
    public void Create_initializes_status_to_pending()
    {
        var image = ListingImage.Create(Guid.NewGuid(), "/uploads/photo.jpg", 0);

        image.Status.Should().Be(PhotoStatus.Pending);
        image.IsPublic.Should().BeFalse();
        image.ModeratedAt.Should().BeNull();
        image.ModeratedByAdminId.Should().BeNull();
    }

    [Fact]
    public void Create_throws_when_file_path_empty()
    {
        var act = () => ListingImage.Create(Guid.NewGuid(), string.Empty, 0);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Moderate_to_public_sets_isPublic_true_and_stamps_admin()
    {
        var image = ListingImage.Create(Guid.NewGuid(), "/uploads/photo.jpg", 0);
        var adminId = Guid.NewGuid();

        image.Moderate(PhotoStatus.Public, adminId);

        image.Status.Should().Be(PhotoStatus.Public);
        image.IsPublic.Should().BeTrue();
        image.ModeratedByAdminId.Should().Be(adminId);
        image.ModeratedAt.Should().NotBeNull();
    }

    [Fact]
    public void Moderate_to_rejected_records_gm_feedback()
    {
        var image = ListingImage.Create(Guid.NewGuid(), "/uploads/photo.jpg", 0);

        image.Moderate(PhotoStatus.Rejected, Guid.NewGuid(), "Blurry image");

        image.Status.Should().Be(PhotoStatus.Rejected);
        image.GmFeedback.Should().Be("Blurry image");
        image.IsPublic.Should().BeFalse();
    }

    [Fact]
    public void Moderate_back_to_pending_is_forbidden()
    {
        var image = ListingImage.Create(Guid.NewGuid(), "/uploads/photo.jpg", 0);

        var act = () => image.Moderate(PhotoStatus.Pending, Guid.NewGuid());

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void UpdateMetadata_does_not_change_moderation_status()
    {
        var image = ListingImage.Create(Guid.NewGuid(), "/uploads/photo.jpg", 0);
        image.Moderate(PhotoStatus.Public, Guid.NewGuid());

        image.UpdateMetadata("alt-en", "alt-es", 5);

        image.Status.Should().Be(PhotoStatus.Public); // Regla #4: reordenar no re-modera
        image.AltEn.Should().Be("alt-en");
        image.AltEs.Should().Be("alt-es");
        image.DisplayOrder.Should().Be(5);
    }
}
