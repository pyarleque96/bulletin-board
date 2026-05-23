namespace Bulletin.Board.Application.Storage;

/// <summary>
/// Persists binary blobs (photos, attachments) and returns a public URL clients can use
/// to fetch them. Implementations are swappable: local filesystem for dev, Azure Blob /
/// S3 for prod. The contract is stable so call sites don't change when migrating.
/// </summary>
public interface IBlobStorage
{
    /// <summary>
    /// Stores the stream contents under <paramref name="containerName"/> and returns the
    /// public URL. The implementation chooses the file name (typically a guid + extension).
    /// </summary>
    /// <param name="containerName">Logical bucket / folder (e.g. "vehicle-photos").</param>
    /// <param name="content">Stream to upload (caller owns lifetime).</param>
    /// <param name="contentType">MIME type (e.g. "image/jpeg"). Validated by the caller.</param>
    /// <param name="extension">File extension WITHOUT the leading dot (e.g. "jpg").</param>
    Task<string> UploadAsync(
        string containerName,
        Stream content,
        string contentType,
        string extension,
        CancellationToken ct = default);

    /// <summary>
    /// Best-effort delete by URL. Returns true if the blob was deleted, false if it didn't
    /// exist or wasn't owned by this storage. Failure to delete must not throw.
    /// </summary>
    Task<bool> DeleteAsync(string url, CancellationToken ct = default);
}
