using FEA.URVP.Application.DTOs.Projects;
using MediatR;

namespace FEA.URVP.Application.Queries.Projects.ListParticipants;

public sealed record ListProjectParticipantsQuery(
    Guid ProjectId,
    Guid CurrentUserId,
    bool IsAdmin) : IRequest<IReadOnlyList<ProjectParticipantDto>>;
