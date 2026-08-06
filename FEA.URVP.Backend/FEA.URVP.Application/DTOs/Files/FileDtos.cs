namespace FEA.URVP.Application.DTOs.Files;

public sealed class FileMetadataDto
{
    public Guid Id { get; init; }
    public string EntityType { get; init; } = null!;
    public Guid EntityId { get; init; }
    public string FileCategory { get; init; } = null!;
    public string FileName { get; init; } = null!;
    public string MimeType { get; init; } = null!;
    public long FileSize { get; init; }
    public DateTime UploadedAt { get; init; }
}

public sealed class FileContentDto
{
    public Guid Id { get; init; }
    public string FileName { get; init; } = null!;
    public string MimeType { get; init; } = null!;
    public byte[] Content { get; init; } = [];
    public byte[] ContentHash { get; init; } = [];
}
