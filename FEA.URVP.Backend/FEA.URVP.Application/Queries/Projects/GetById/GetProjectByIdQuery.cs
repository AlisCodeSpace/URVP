using FEA.URVP.Application.DTOs.Projects;
using MediatR;

namespace FEA.URVP.Application.Queries.Projects.GetById;

public sealed record GetProjectByIdQuery(
    Guid ProjectId,
    Guid ViewerUserId,
    bool ViewerIsAdmin,
    bool ViewerIsStudent) : IRequest<ProjectDto>;
