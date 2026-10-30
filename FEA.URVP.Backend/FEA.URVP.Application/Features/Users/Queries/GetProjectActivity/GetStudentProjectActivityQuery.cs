using FEA.URVP.Application.DTOs.Users;
using MediatR;

namespace FEA.URVP.Application.Queries.Users.GetProjectActivity;

public sealed record GetStudentProjectActivityQuery(Guid StudentUserId)
    : IRequest<StudentProjectActivityDto>;
