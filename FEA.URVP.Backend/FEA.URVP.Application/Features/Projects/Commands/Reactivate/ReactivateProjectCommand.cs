using FEA.URVP.Application.Features.Projects.DTOs;
using MediatR;

namespace FEA.URVP.Application.Features.Projects.Commands.Reactivate;

public sealed record ReactivateProjectCommand(
    Guid ProjectId,
    Guid CurrentUserId,
    bool IsAdmin) : IRequest<ProjectDto>;
