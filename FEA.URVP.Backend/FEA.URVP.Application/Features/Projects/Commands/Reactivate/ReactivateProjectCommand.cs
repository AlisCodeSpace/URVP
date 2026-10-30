using FEA.URVP.Application.DTOs.Projects;
using MediatR;

namespace FEA.URVP.Application.Commands.Projects.Reactivate;

public sealed record ReactivateProjectCommand(
    Guid ProjectId,
    Guid CurrentUserId,
    bool IsAdmin) : IRequest<ProjectDto>;
