using System.Net;

namespace Bulletin.Board.Web.Client.Services.Common;

/// <summary>
/// Discriminated-union-like result type for all typed HTTP service calls.
/// Eliminates null-return ambiguity and propagates structured error information
/// so UI components can render the correct state (Loading, Success, Error) without
/// any implicit null checks.
/// </summary>
public abstract record ApiResult<T>
{
    /// <summary>Request is in flight.</summary>
    public sealed record Loading : ApiResult<T>;

    /// <summary>Request completed successfully.</summary>
    public sealed record Success(T Data) : ApiResult<T>;

    /// <summary>
    /// Request failed. <see cref="HttpStatus"/> is null for network/timeout errors.
    /// <see cref="MessageKey"/> maps to an i18n resource key in SharedResource.
    /// <see cref="RawDetail"/> contains the raw Problem Details message when available.
    /// </summary>
    public sealed record Error(int? HttpStatus, string MessageKey, string? RawDetail) : ApiResult<T>;

    // ── Factory helpers ────────────────────────────────────────────────────────

    /// <summary>Creates a Loading instance.</summary>
    public static ApiResult<T> IsLoading() => new Loading();

    /// <summary>Creates a Success instance wrapping <paramref name="data"/>.</summary>
    public static ApiResult<T> Ok(T data) => new Success(data);

    /// <summary>
    /// Maps an <see cref="HttpStatusCode"/> to a structured Error instance.
    /// 5xx codes all map to <c>errors.server</c>.
    /// </summary>
    public static ApiResult<T> FromStatusCode(HttpStatusCode statusCode, string? rawDetail = null)
    {
        var messageKey = MapStatusCode(statusCode);
        return new Error((int)statusCode, messageKey, rawDetail);
    }

    /// <summary>Creates a network/timeout Error (no HTTP status).</summary>
    public static ApiResult<T> NetworkError(string? rawDetail = null)
        => new Error(null, "errors.network", rawDetail);

    // ── Convenience pattern matching ───────────────────────────────────────────

    /// <summary>Returns true when this is a <see cref="Success"/> instance.</summary>
    public bool IsSuccess => this is Success;

    /// <summary>Returns the data when successful; otherwise default.</summary>
    public T? DataOrDefault => this is Success s ? s.Data : default;

    // ── Status → key mapping ───────────────────────────────────────────────────

    private static string MapStatusCode(HttpStatusCode code) =>
        (int)code switch
        {
            401             => "errors.unauthorized",
            403             => "errors.forbidden",
            404             => "errors.not_found",
            409             => "errors.conflict",
            422             => "errors.validation",
            >= 500 and < 600 => "errors.server",
            _               => "errors.unknown",
        };
}
