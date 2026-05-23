using Bulletin.Board.Application.Settings;
using Bulletin.Board.Application.Snapshots;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bulletin.Board.Application.Commands.Admin.Listings;

public record ApproveListingCommand(Guid ListingId, string? FeedbackEn, string? FeedbackEs) : IRequest;

public sealed class ApproveListingCommandHandler(
    IListingRepository listingRepository,
    IListingSnapshotBuilder snapshotBuilder,
    ICurrentUserService currentUserService,
    IEmailService emailService,
    IOptions<NotificationSettings> notificationOptions,
    ILogger<ApproveListingCommandHandler> logger)
    : IRequestHandler<ApproveListingCommand>
{
    public async Task Handle(ApproveListingCommand request, CancellationToken ct)
    {
        // Load with the navigations the snapshot builder needs (images + vehicles + photos).
        var listing = await listingRepository.GetForSnapshotAsync(request.ListingId, ct)
            ?? throw new InvalidOperationException($"Listing '{request.ListingId}' not found.");

        var adminId = currentUserService.UserId ?? throw new UnauthorizedAccessException();

        // Capture the current state as the published snapshot. Public reads project from this
        // so the listing keeps showing the just-approved version while the provider edits next.
        var snapshotJson = snapshotBuilder.Build(listing);

        listing.Approve(adminId, snapshotJson, ListingSnapshot.CurrentVersion);
        listingRepository.Update(listing);
        await listingRepository.SaveChangesAsync(ct);

        logger.LogInformation("Listing {ListingId} approved by {AdminId}", request.ListingId, adminId);

        var gmEmail = notificationOptions.Value.GmEmail;
        try
        {
            await emailService.SendAsync(
                to: gmEmail,
                subject: $"[Bulletin Dells] Listing approved: {listing.TitleEn}",
                htmlBody: $"""
                    <h2>Listing Approved</h2>
                    <p><strong>Listing:</strong> {listing.TitleEn}</p>
                    <p><strong>Listing ID:</strong> {listing.Id}</p>
                    <p><strong>Approved by:</strong> {adminId}</p>
                    <p><strong>Time:</strong> {DateTimeOffset.UtcNow:u}</p>
                    """,
                ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send approval notification for listing {ListingId}", request.ListingId);
        }
    }
}

public sealed class ApproveListingCommandValidator : AbstractValidator<ApproveListingCommand>
{
    public ApproveListingCommandValidator()
    {
        RuleFor(x => x.ListingId).NotEmpty();
    }
}
