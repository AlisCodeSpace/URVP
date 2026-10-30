using FEA.URVP.Application.Features.Projects.DTOs;
using MediatR;

namespace FEA.URVP.Application.Features.Projects.Queries.ListAssistedProjects;

/// <summary>Projects on which the signed-in user is listed as a research assistant, matched by email.</summary>
public sealed record ListAssistedProjectsQuery(Guid CurrentUserId)
    : IRequest<IReadOnlyList<AssistedProjectDto>>;
