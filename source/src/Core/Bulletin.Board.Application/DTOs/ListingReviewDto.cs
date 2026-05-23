namespace Bulletin.Board.Application.DTOs;

/// <summary>
/// A single approved review visible in the owner's listing dashboard.
/// </summary>
public record ListingReviewDto(
    Guid Id,
    string ReviewerName,
    decimal Rating,
    string? Comment,
    DateTimeOffset CreatedAt,
    bool IsVerified);
