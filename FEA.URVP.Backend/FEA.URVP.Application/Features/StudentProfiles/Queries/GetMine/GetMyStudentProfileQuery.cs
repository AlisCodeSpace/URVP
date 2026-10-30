using FEA.URVP.Application.DTOs.StudentProfiles;
using MediatR;

namespace FEA.URVP.Application.Queries.StudentProfiles.GetMine;

public sealed class GetMyStudentProfileQuery : IRequest<StudentProfileDto>
{
    public Guid CurrentUserId { get; }

    public GetMyStudentProfileQuery(Guid currentUserId)
    {
        CurrentUserId = currentUserId;
    }
}
