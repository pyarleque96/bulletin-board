using Bulletin.Board.Application.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Infrastructure.Storage;

/// <summary>
/// Dev-grade <see cref="IBlobStorage"/> that writes to <c>wwwroot/uploads/{container}/</c>.
/// </summary>
/// <remarks>
/// NOT production-ready: files don't survive multi-instance deploys, no CDN, no caching headers,
/// no anti-virus scan. For prod swap with an Azure Blob / S3 implementation behind the same interface.
/// The URLs returned are absolute paths (e.g. <c>/uploads/vehicle-photos/abc.jpg</c>) so the
/// browser fetches them directly from the API host's static files.
/// </remarks>
public sealed class LocalFilesystemBlobStorage(
    IWebHostEnvironment env,
    IHttpContextAccessor httpContextAccessor,
    ILogger<LocalFilesystemBlobStorage> logger)
    : IBlobStorage
{
    private const string UploadsRoot = "uploads";

    public async Task<string> UploadAsync(
        string containerName,
        Stream content,
        string contentType,
        string extension,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(containerName);
        ArgumentException.ThrowIfNullOrWhiteSpace(extension);
        ArgumentNullException.ThrowIfNull(content);

        // Sanitize: container becomes a single safe path segment; extension stays alphanumeric.
        var safeContainer = SanitizeSegment(containerName);
        var safeExt = SanitizeExtension(extension);

        var fileName = $"{Guid.NewGuid():N}.{safeExt}";
        var relativeDir = Path.Combine(UploadsRoot, safeContainer);
        var absoluteDir = Path.Combine(env.WebRootPath, relativeDir);
        Directory.CreateDirectory(absoluteDir);

        var absolutePath = Path.Combine(absoluteDir, fileName);
        await using (var fs = File.Create(absolutePath))
        {
            await content.CopyToAsync(fs, ct);
        }

        // Build the URL absolute to the API origin so the Web client (different host) can load it.
        var path = $"/{UploadsRoot}/{safeContainer}/{fileName}";
        var url = ResolveAbsolute(path);
        logger.LogInformation("Uploaded blob to {Url} ({Size} bytes)", url, new FileInfo(absolutePath).Length);
        return url;
    }

    private string ResolveAbsolute(string relativePath)
    {
        var request = httpContextAccessor.HttpContext?.Request;
        if (request is null)
            return relativePath;  // background/test contexts — leave relative
        return $"{request.Scheme}://{request.Host}{relativePath}";
    }

    public Task<bool> DeleteAsync(string url, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(url)) return Task.FromResult(false);

        // Accept either a relative path or an absolute URL — extract the path portion.
        string pathPart;
        if (Uri.TryCreate(url, UriKind.Absolute, out var parsed))
            pathPart = parsed.AbsolutePath;
        else
            pathPart = url;

        if (!pathPart.StartsWith($"/{UploadsRoot}/", StringComparison.Ordinal))
            return Task.FromResult(false);

        var relative = pathPart.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var absolutePath = Path.Combine(env.WebRootPath, relative);

        try
        {
            if (File.Exists(absolutePath))
            {
                File.Delete(absolutePath);
                logger.LogInformation("Deleted blob at {Url}", url);
                return Task.FromResult(true);
            }
        }
        catch (Exception ex)
        {
            // Best-effort — log and move on. Caller already removed the DB row pointing here.
            logger.LogWarning(ex, "Failed to delete blob at {Url}", url);
        }
        return Task.FromResult(false);
    }

    private static string SanitizeSegment(string s)
    {
        var safe = new string([.. s.ToLowerInvariant().Where(c => char.IsLetterOrDigit(c) || c == '-')]);
        return string.IsNullOrEmpty(safe) ? "misc" : safe;
    }

    private static string SanitizeExtension(string ext)
    {
        var safe = new string([.. ext.TrimStart('.').ToLowerInvariant().Where(char.IsLetterOrDigit)]);
        return string.IsNullOrEmpty(safe) ? "bin" : safe;
    }
}
