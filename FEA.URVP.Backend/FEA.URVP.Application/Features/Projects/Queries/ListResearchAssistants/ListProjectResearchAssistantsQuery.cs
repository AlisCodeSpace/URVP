using FEA.URVP.Application.Features.Projects.DTOs;
using MediatR;

namespace FEA.URVP.Application.Features.Projects.Queries.ListResearchAssistants;

public sealed record ListProjectResearchAssistantsQuery(
    Guid ProjectId,
    Guid CurrentUserId,
    bool IsAdmin) : IRequest<IReadOnlyList<ProjectResearchAssistantDto>>;
