using FEA.URVP.Application.Features.Projects.DTOs;
using MediatR;

namespace FEA.URVP.Application.Features.Projects.Queries.ListAlumni;

public sealed record ListProjectAlumniQuery(
    Guid ProjectId,
    Guid CurrentUserId,
    bool IsAdmin) : IRequest<IReadOnlyList<ProjectAlumniDto>>;
