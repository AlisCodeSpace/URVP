namespace FEA.URVP.Application.Features.Users.DTOs;

public sealed class UserExportFileDto
{
    public required byte[] Content { get; init; }
    public required string MimeType { get; init; }
    public required string FileName { get; init; }
}
