using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.DTOs.Projects;
using FEA.URVP.Application.Mappings;
using FEA.URVP.Application.Projects;
using MediatR;

namespace FEA.URVP.Application.Queries.Projects.ListResearchAssistants;

public sealed class ListProjectResearchAssistantsQueryHandler
    : IRequestHandler<ListProjectResearchAssistantsQuery, IReadOnlyList<ProjectResearchAssistantDto>>
{
    private readonly IProjectRepository _projects;
    private readonly IProjectResearchAssistantRepository _assistants;

    public ListProjectResearchAssistantsQueryHandler(
        IProjectRepository projects,
        IProjectResearchAssistantRepository assistants)
    {
        _projects = projects;
        _assistants = assistants;
    }

    public async Task<IReadOnlyList<ProjectResearchAssistantDto>> Handle(
        ListProjectResearchAssistantsQuery request,
        CancellationToken cancellationToken)
    {
        var project = await _projects.FindByIdAsync(request.ProjectId, cancellationToken)
            ?? throw new KeyNotFoundException($"Project {request.ProjectId} was not found.");

        ProjectResearchAssistantAccess.EnsureCanManage(project, request.CurrentUserId, request.IsAdmin);

        var assistants = await _assistants.ListByProjectAsync(project.Id, cancellationToken);
        var names = await _assistants.NamesForEmailsAsync(
            assistants.Select(assistant => assistant.Email).ToList(),
            cancellationToken);

        return assistants.Select(assistant => assistant.ToDto(names)).ToList();
    }
}
