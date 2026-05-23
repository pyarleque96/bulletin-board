using Bulletin.Board.Domain.Enums;

namespace Bulletin.Board.Domain.Entities;

public class Report
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid? ProviderId { get; private set; }
    public Guid? ListingId { get; private set; }
    public Guid ReportedById { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public ReportStatus Status { get; private set; } = ReportStatus.Pending;
    public Guid? ReviewedById { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public Provider? Provider { get; private set; }
    public Listing? Listing { get; private set; }

    private Report() { }

    public static Report Create(Guid reportedById, string reason, Guid? providerId = null, Guid? listingId = null)
    {
        if (providerId is null && listingId is null)
            throw new ArgumentException("Report must target a provider or a listing.");

        return new Report
        {
            ReportedById = reportedById,
            Reason = reason,
            ProviderId = providerId,
            ListingId = listingId
        };
    }

    public void Review(Guid reviewerId) { Status = ReportStatus.Reviewed; ReviewedById = reviewerId; }
    public void Dismiss(Guid reviewerId) { Status = ReportStatus.Dismissed; ReviewedById = reviewerId; }
}
