namespace Bulletin.Board.Application.DTOs;

/// <summary>
/// Review DTO for /manage Reviews tab. Frontend ya conoce los strings de estado:
/// "Published" | "PendingGm" | "Rejected".
/// </summary>
public record ManageReviewDto(
    Guid Id,
    string ClientDisplayName,
    string? ClientAvatarUrl,
    int Rating,
    string Comment,
    DateTimeOffset CreatedAt,
    string Status,
    string? GmFeedback,
    ReviewReplyDto? ProviderReply);

public record ReviewReplyDto(
    Guid Id,
    string Text,
    string Status,
    DateTimeOffset CreatedAt);

public record RatingSummaryDto(
    double Average,
    int Total,
    IDictionary<int, int> Distribution);
