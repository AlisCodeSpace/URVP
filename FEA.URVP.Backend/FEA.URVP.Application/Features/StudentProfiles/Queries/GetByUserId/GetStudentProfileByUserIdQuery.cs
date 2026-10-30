using FEA.URVP.Application.Features.StudentProfiles.DTOs;
using MediatR;

namespace FEA.URVP.Application.Features.StudentProfiles.Queries.GetByUserId;

public sealed class GetStudentProfileByUserIdQuery : IRequest<StudentProfileDto>
{
    public Guid CurrentUserId { get; }
    public Guid StudentUserId { get; }

    public GetStudentProfileByUserIdQuery(Guid currentUserId, Guid studentUserId)
    {
        CurrentUserId = currentUserId;
        StudentUserId = studentUserId;
    }
}
