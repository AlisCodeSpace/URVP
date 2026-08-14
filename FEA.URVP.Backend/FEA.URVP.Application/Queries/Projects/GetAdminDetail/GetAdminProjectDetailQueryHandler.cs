using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.DTOs.Projects;
using FEA.URVP.Application.Mappings;
using MediatR;

namespace FEA.URVP.Application.Queries.Projects.GetAdminDetail;

public sealed class GetAdminProjectDetailQueryHandler
    : IRequestHandler<GetAdminProjectDetailQuery, AdminProjectDetailDto>
{
    private readonly IProjectRepository _projects;
    private readonly IProjectRankingRepository _rankings;

    public GetAdminProjectDetailQueryHandler(
        IProjectRepository projects,
        IProjectRankingRepository rankings)
    {
        _projects = projects;
        _rankings = rankings;
    }

    public async Task<AdminProjectDetailDto> Handle(
        GetAdminProjectDetailQuery request,
        CancellationToken cancellationToken)
    {
        var project = await _projects.FindByIdAsync(request.ProjectId, cancellationToken)
            ?? throw new KeyNotFoundException($"Project {request.ProjectId} was not found.");

        var rankings = await _rankings.ListByProjectAsync(project.Id, cancellationToken);

        return new AdminProjectDetailDto
        {
            Project = project.ToDto(),
            Rankings = rankings.Select(r => r.ToStudentDto()).ToList(),
        };
    }
}
