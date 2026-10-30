using FEA.URVP.Application.Features.Files.DTOs;
using MediatR;

namespace FEA.URVP.Application.Features.Files.Queries.GetById;

public sealed class GetFileByIdQuery : IRequest<FileContentDto>
{
    public Guid FileId { get; }
    public Guid CurrentUserId { get; }
    public bool IsAdmin { get; }

    public GetFileByIdQuery(Guid fileId, Guid currentUserId, bool isAdmin)
    {
        FileId = fileId;
        CurrentUserId = currentUserId;
        IsAdmin = isAdmin;
    }
}
