namespace Bulletin.Board.Domain.Entities;

/// <summary>
/// Append-only view event for a listing. Powers the analytics tab and the
/// <c>views_30d</c> term of the trending score.
///
/// <para>Throttling lives at the application layer (in-memory dedupe per
/// session_hash, 5-minute window) — the DB allows duplicates so a hammering
/// client cannot fail the request, just stops accumulating new rows.</para>
/// </summary>
public class ListingView
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ListingId { get; private set; }
    public string SessionHash { get; private set; } = string.Empty;
    public Guid? UserId { get; private set; }
    public DateTimeOffset ViewedAt { get; private set; } = DateTimeOffset.UtcNow;

    private ListingView() { }

    public static ListingView Create(Guid listingId, string sessionHash, Guid? userId = null)
        => new()
        {
            ListingId = listingId,
            SessionHash = sessionHash,
            UserId = userId
        };
}
