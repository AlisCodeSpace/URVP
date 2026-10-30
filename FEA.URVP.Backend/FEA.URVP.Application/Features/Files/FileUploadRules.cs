using FEA.URVP.Application.Options;
using FEA.URVP.Domain.Catalog;
using Microsoft.AspNetCore.Http;

namespace FEA.URVP.Application.Files;

/// <summary>
/// Shared signature, extension, and size rules used by the upload handler.
/// This is not a second upload service — it is the policy the existing
/// <c>UploadFileCommandHandler</c> applies after a single MIME detection.
/// </summary>
public static class FileUploadRules
{
    public static bool IsAllowedPdfMime(string? mime) =>
        !string.IsNullOrWhiteSpace(mime)
        && FileStorageCatalog.AllowedPdfMimeTypes.Contains(mime);

    public static bool IsAllowedImageMime(string? mime) =>
        !string.IsNullOrWhiteSpace(mime)
        && FileStorageCatalog.AllowedImageMimeTypes.Contains(mime);

    public static void EnsureAccepted(
        string? detectedMime,
        string fileName,
        long length,
        bool image,
        FileStorageOptions options)
    {
        if (string.IsNullOrWhiteSpace(fileName) || length <= 0)
        {
            throw new ArgumentException("File is required.");
        }

        var extension = Path.GetExtension(fileName);

        if (image)
        {
            if (!IsAllowedImageMime(detectedMime))
            {
                throw new ArgumentException("Only JPEG, PNG, or GIF images are allowed.");
            }

            if (!FileStorageCatalog.AllowedImageExtensions.Contains(extension))
            {
                throw new ArgumentException("Only JPG, JPEG, PNG, or GIF files are allowed.");
            }

            if (length > options.MaxImageSizeBytes)
            {
                throw new ArgumentException(
                    $"Image size must not exceed {options.MaxImageSizeBytes / (1024 * 1024)} MB.");
            }

            return;
        }

        if (!IsAllowedPdfMime(detectedMime))
        {
            throw new ArgumentException("Only PDF files are allowed.");
        }

        if (!FileStorageCatalog.AllowedPdfExtensions.Contains(extension))
        {
            throw new ArgumentException("Only PDF files are allowed.");
        }

        if (length > options.MaxPdfSizeBytes)
        {
            throw new ArgumentException(
                $"File size must not exceed {options.MaxPdfSizeBytes / (1024 * 1024)} MB.");
        }
    }

    public static void EnsureAccepted(
        IReadOnlyList<(string? DetectedMime, string FileName, long Length)> files,
        bool image,
        FileStorageOptions options)
    {
        if (files is null || files.Count == 0)
        {
            throw new ArgumentException("At least one file is required.");
        }

        long total = 0;
        foreach (var file in files)
        {
            if (string.IsNullOrWhiteSpace(file.FileName) || file.Length <= 0)
            {
                throw new ArgumentException("File is required.");
            }

            total += file.Length;
            EnsureAccepted(file.DetectedMime, file.FileName, file.Length, image, options);
        }

        if (total > options.MaxTotalSizeBytes)
        {
            throw new ArgumentException(
                $"Total upload size must not exceed {options.MaxTotalSizeBytes / (1024 * 1024)} MB.");
        }
    }

    public static void EnsureAccepted(
        IReadOnlyList<(string? DetectedMime, IFormFile File)> files,
        bool image,
        FileStorageOptions options)
    {
        if (files is null || files.Count == 0)
        {
            throw new ArgumentException("At least one file is required.");
        }

        var snapshots = new List<(string? DetectedMime, string FileName, long Length)>(files.Count);
        foreach (var (detectedMime, file) in files)
        {
            if (file is null || file.Length <= 0 || string.IsNullOrWhiteSpace(file.FileName))
            {
                throw new ArgumentException("File is required.");
            }

            snapshots.Add((detectedMime, file.FileName, file.Length));
        }

        EnsureAccepted(snapshots, image, options);
    }
}
