using FEA.URVP.Application.Features.Users.DTOs;
using MediatR;

namespace FEA.URVP.Application.Features.Users.Queries.GetProjectActivity;

public sealed record GetStudentProjectActivityQuery(Guid StudentUserId)
    : IRequest<StudentProjectActivityDto>;
