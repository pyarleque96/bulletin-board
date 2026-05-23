using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using FluentAssertions;

namespace Bulletin.Board.Domain.Tests.Entities;

/// <summary>
/// Regla #8: cualquier interacción cliente↔proveedor se loguea en ContactLog y dispara
/// email al GM. El dominio garantiza el log al marcar como Contacted.
/// </summary>
public class InquiryTests
{
    [Fact]
    public void Create_initializes_to_new_status()
    {
        var inquiry = Inquiry.Create(
            listingId: Guid.NewGuid(),
            providerId: Guid.NewGuid(),
            initiatorUserId: Guid.NewGuid(),
            clientName: "Alice",
            clientWhatsAppPhone: "+15551234567",
            clientLanguage: "en",
            subject: "Question",
            messagePreview: "Hi");

        inquiry.Status.Should().Be(InquiryStatus.New);
        inquiry.IsRead.Should().BeFalse();
        inquiry.ContactLogs.Should().BeEmpty();
    }

    [Fact]
    public void MarkRead_transitions_to_read()
    {
        var inquiry = MakeInquiry();
        inquiry.MarkRead();

        inquiry.IsRead.Should().BeTrue();
        inquiry.Status.Should().Be(InquiryStatus.Read);
    }

    [Fact]
    public void MarkContacted_appends_contact_log_for_audit()
    {
        var inquiry = MakeInquiry();

        var log = inquiry.MarkContacted(ContactMethod.WhatsApp, "Provider replied via WA");

        inquiry.Status.Should().Be(InquiryStatus.Contacted);
        inquiry.ContactedAt.Should().NotBeNull();
        inquiry.ContactLogs.Should().ContainSingle();
        log.Method.Should().Be(ContactMethod.WhatsApp);
        log.GmNotifiedAt.Should().BeNull(); // background job is responsible
    }

    private static Inquiry MakeInquiry() => Inquiry.Create(
        listingId: Guid.NewGuid(),
        providerId: Guid.NewGuid(),
        initiatorUserId: Guid.NewGuid(),
        clientName: "Alice",
        clientWhatsAppPhone: "+15551234567",
        clientLanguage: "en",
        subject: "Question",
        messagePreview: "Hi");
}
