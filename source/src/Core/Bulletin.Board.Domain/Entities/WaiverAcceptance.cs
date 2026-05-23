namespace Bulletin.Board.Domain.Entities;

public class WaiverAcceptance
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public DateTimeOffset AcceptedAt { get; private set; } = DateTimeOffset.UtcNow;
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }

    private WaiverAcceptance() { }

    public static WaiverAcceptance Create(Guid userId, string? ipAddress = null, string? userAgent = null)
        => new() { UserId = userId, IpAddress = ipAddress, UserAgent = userAgent };
}
