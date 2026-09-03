using System.Text.Json.Serialization;
using FEA.URVP.Application.DTOs.Files;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FEA.URVP.Application.Commands.Files.Upload;

public sealed class UploadFileCommand : IRequest<FileMetadataDto>
{
    [JsonIgnore]
    public Guid CurrentUserId { get; set; }

    public string EntityType { get; init; } = null!;
    public Guid EntityId { get; init; }
    public string FileCategory { get; init; } = null!;

    [JsonIgnore]
    public IFormFile File { get; init; } = null!;
}
