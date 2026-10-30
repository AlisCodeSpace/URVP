using FEA.URVP.Application.DTOs.Projects;
using MediatR;

namespace FEA.URVP.Application.Queries.Projects.ListAssistedProjects;

/// <summary>Projects on which the signed-in user is listed as a research assistant, matched by email.</summary>
public sealed record ListAssistedProjectsQuery(Guid CurrentUserId)
    : IRequest<IReadOnlyList<AssistedProjectDto>>;
