using FEA.URVP.Application.Features.Projects.DTOs;
using MediatR;

namespace FEA.URVP.Application.Features.Projects.Queries.GetById;

public sealed record GetProjectByIdQuery(
    Guid ProjectId,
    Guid ViewerUserId,
    bool ViewerIsAdmin,
    bool ViewerIsStudent) : IRequest<ProjectDto>;
