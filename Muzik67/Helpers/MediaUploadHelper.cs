using Microsoft.AspNetCore.Http;

namespace Muzik67.Helpers;

public static class MediaUploadHelper
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp3", ".wav", ".m4a", ".ogg", ".png", ".jpg", ".jpeg", ".gif", ".webp", ".pdf", ".mp4"
    };

    private static readonly HashSet<string> AudioExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp3", ".wav", ".m4a", ".ogg"
    };

    public static async Task<string> SaveAsync(
        IFormFile file,
        IWebHostEnvironment environment,
        bool audioOnly = false,
        long maxBytes = 12 * 1024 * 1024)
    {
        if (file.Length <= 0 || file.Length > maxBytes)
        {
            throw new InvalidOperationException($"The media file must be smaller than {maxBytes / 1024 / 1024} MB.");
        }

        var extension = Path.GetExtension(file.FileName);
        var allowed = audioOnly ? AudioExtensions : AllowedExtensions;
        if (string.IsNullOrWhiteSpace(extension) || !allowed.Contains(extension))
        {
            throw new InvalidOperationException(audioOnly
                ? "Upload an MP3, WAV, M4A, or OGG audio file."
                : "Upload an MP3, WAV, M4A, OGG, PNG, JPG, GIF, WEBP, PDF, or MP4 file.");
        }

        var uploadRoot = Path.Combine(environment.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadRoot);
        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var targetPath = Path.Combine(uploadRoot, fileName);
        await using var stream = new FileStream(targetPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await file.CopyToAsync(stream);
        return $"/uploads/{fileName}";
    }

    public static bool IsAudio(string? url) => AudioExtensions.Contains(Path.GetExtension(url ?? string.Empty));
    public static bool IsImage(string? url) => new[] { ".png", ".jpg", ".jpeg", ".gif", ".webp" }
        .Contains(Path.GetExtension(url ?? string.Empty), StringComparer.OrdinalIgnoreCase);
    public static bool IsVideo(string? url) => string.Equals(Path.GetExtension(url ?? string.Empty), ".mp4", StringComparison.OrdinalIgnoreCase);
}
