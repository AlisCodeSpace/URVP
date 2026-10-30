using FEA.URVP.Application.Features.StudentProfiles.DTOs;
using MediatR;

namespace FEA.URVP.Application.Features.StudentProfiles.Queries.GetMine;

public sealed class GetMyStudentProfileQuery : IRequest<StudentProfileDto>
{
    public Guid CurrentUserId { get; }

    public GetMyStudentProfileQuery(Guid currentUserId)
    {
        CurrentUserId = currentUserId;
    }
}
