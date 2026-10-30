using FEA.URVP.Application.Features.Projects.DTOs;
using MediatR;

namespace FEA.URVP.Application.Features.Projects.Queries.ListParticipants;

public sealed record ListProjectParticipantsQuery(
    Guid ProjectId,
    Guid CurrentUserId,
    bool IsAdmin) : IRequest<IReadOnlyList<ProjectParticipantDto>>;
