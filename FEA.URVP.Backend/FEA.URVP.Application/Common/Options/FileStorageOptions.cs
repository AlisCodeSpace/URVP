namespace FEA.URVP.Application.Options;

public sealed class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    public long MaxImageSizeBytes { get; set; } = 2_097_152;

    public long MaxPdfSizeBytes { get; set; } = 10_485_760;

    public long MaxTotalSizeBytes { get; set; } = 26_214_400;
}
