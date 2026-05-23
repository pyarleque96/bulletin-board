using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Application.Settings;
using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bulletin.Board.Application.Commands.Contacts;

public sealed class ContactProviderCommandHandler(
    IListingRepository listingRepository,
    IRepository<Contact> contactRepository,
    IRepository<WaiverAcceptance> waiverRepository,
    ICurrentUserService currentUserService,
    IEmailService emailService,
    IOptions<NotificationSettings> notificationOptions,
    ILogger<ContactProviderCommandHandler> logger)
    : IRequestHandler<ContactProviderCommand, ContactResultDto>
{
    public async Task<ContactResultDto> Handle(ContactProviderCommand request, CancellationToken ct)
    {
        // Regla de negocio: waiver debe estar aceptado
        if (!request.WaiverAccepted)
            throw new InvalidOperationException("You must accept the waiver before contacting a provider.");

        var listing = await listingRepository.GetByIdAsync(request.ListingId, ct);
        if (listing is null || listing.IsDeleted || listing.Status != ListingStatus.Approved)
            throw new InvalidOperationException("Listing not found or not available.");

        var userId = currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User must be authenticated to contact a provider.");

        // Registrar waiver si no existe aún para este usuario
        var existingWaivers = await waiverRepository.FindAsync(w => w.UserId == userId, ct);
        if (!existingWaivers.Any())
        {
            var waiver = WaiverAcceptance.Create(userId);
            await waiverRepository.AddAsync(waiver, ct);
        }

        // Crear el registro de contacto (regla: rating solo de usuarios que contactaron)
        var contact = Contact.Create(
            listingId: listing.Id,
            initiatorUserId: userId,
            providerId: listing.ProviderId);

        await contactRepository.AddAsync(contact, ct);
        await contactRepository.SaveChangesAsync(ct);

        // Construir URL de WhatsApp
        var whatsAppNumber = listing.WhatsAppNumber ?? listing.Provider?.WhatsAppNumber ?? string.Empty;
        var cleanNumber = whatsAppNumber.Replace("+", "").Replace(" ", "").Replace("-", "");
        var message = Uri.EscapeDataString("Hi! I found your listing on Bulletin Dells and I'm interested.");
        var whatsAppUrl = $"https://wa.me/{cleanNumber}?text={message}";

        // Notificar al GM por email
        var gmEmail = notificationOptions.Value.GmEmail;
        try
        {
            await emailService.SendAsync(
                to: gmEmail,
                subject: $"[Bulletin Dells] New contact for listing: {listing.TitleEn}",
                htmlBody: $"""
                    <h2>New Contact Interaction</h2>
                    <p><strong>Listing:</strong> {listing.TitleEn}</p>
                    <p><strong>Listing ID:</strong> {listing.Id}</p>
                    <p><strong>User ID:</strong> {userId}</p>
                    <p><strong>Time:</strong> {DateTimeOffset.UtcNow:u}</p>
                    <p>The user has been directed to WhatsApp.</p>
                    """,
                ct);
        }
        catch (Exception ex)
        {
            // No fallar la operación si el email no se puede enviar
            logger.LogError(ex, "Failed to send GM notification email for contact on listing {ListingId}", listing.Id);
        }

        return new ContactResultDto(whatsAppUrl);
    }
}
