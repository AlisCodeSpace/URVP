using FEA.URVP.Application.DTOs.StudentProfiles;
using MediatR;

namespace FEA.URVP.Application.Queries.StudentProfiles.GetByUserId;

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
