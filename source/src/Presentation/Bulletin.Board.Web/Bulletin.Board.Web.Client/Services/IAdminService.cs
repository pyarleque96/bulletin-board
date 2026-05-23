using Bulletin.Board.Web.Client.Models;

namespace Bulletin.Board.Web.Client.Services;

/// <summary>
/// Contract for admin-only operations.  All methods require a JWT
/// with role "Admin" — the BFF proxy attaches the JWT from the auth cookie.
/// </summary>
public interface IAdminService
{
    Task<AdminStatsDto?> GetStatsAsync(CancellationToken ct = default);

    Task<PagedResult<AdminListingDto>> GetListingsAsync(
        string? status,
        string? tier,
        Guid? categoryId,
        int page,
        int pageSize,
        Guid? providerId = null,
        CancellationToken ct = default);

    Task ApproveListingAsync(Guid id, string? feedbackEn = null, string? feedbackEs = null, CancellationToken ct = default);

    Task RejectListingAsync(Guid id, string reason, CancellationToken ct = default);

    Task RequestChangesAsync(Guid id, string feedback, CancellationToken ct = default);

    Task DeleteListingAsync(Guid id, CancellationToken ct = default);

    Task<PagedResult<AdminRatingDto>> GetRatingsAsync(
        string? status,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task ApproveRatingAsync(Guid id, CancellationToken ct = default);

    Task RejectRatingAsync(Guid id, CancellationToken ct = default);

    Task<PagedResult<AdminProviderDto>> GetProvidersAsync(
        string? tier,
        string? verificationStatus,
        int page,
        int pageSize,
        string? sortBy = null,
        string? sortDir = null,
        CancellationToken ct = default);

    Task ChangeProviderTierAsync(Guid id, string tier, CancellationToken ct = default);

    Task SetProviderHierarchyAsync(Guid id, int hierarchy, CancellationToken ct = default);

    /// <summary>
    /// Forces an immediate recompute of every listing's <c>trending_score</c>.
    /// Otherwise the hourly background job applies the update. Returns the number
    /// of listings updated and the elapsed time in milliseconds.
    /// </summary>
    Task<RecomputeTrendingResponse?> RecomputeTrendingAsync(CancellationToken ct = default);

    // ── VIP toggle ───────────────────────────────────────────
    Task<ReauthResponse?> ReauthAsync(string password, CancellationToken ct = default);

    Task GrantProviderVipAsync(Guid providerId, GrantVipRequest body, string reauthToken, CancellationToken ct = default);
    Task RevokeProviderVipAsync(Guid providerId, RevokeVipRequest body, string reauthToken, CancellationToken ct = default);

    Task GrantListingVipAsync(Guid listingId, GrantVipRequest body, string reauthToken, CancellationToken ct = default);
    Task RevokeListingVipAsync(Guid listingId, RevokeVipRequest body, string reauthToken, CancellationToken ct = default);

    Task<IReadOnlyList<VipChangeLogDto>> GetVipLogsAsync(VipEntityKind entityType, Guid entityId, int page = 1, int pageSize = 50, CancellationToken ct = default);
}
