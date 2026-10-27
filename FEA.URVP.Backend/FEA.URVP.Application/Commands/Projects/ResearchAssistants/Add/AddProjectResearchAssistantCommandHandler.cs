using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using FEA.URVP.Application.DTOs.Projects;
using FEA.URVP.Application.Mappings;
using FEA.URVP.Application.Projects;
using FEA.URVP.Domain.Entities.Projects;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Projects.ResearchAssistants.Add;

public sealed class AddProjectResearchAssistantCommandHandler
    : BaseCommandHandler<AddProjectResearchAssistantCommand, ProjectResearchAssistantDto>
{
    private readonly IProjectRepository _projects;
    private readonly IProjectResearchAssistantRepository _assistants;

    public AddProjectResearchAssistantCommandHandler(
        ILogger<AddProjectResearchAssistantCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IProjectRepository projects,
        IProjectResearchAssistantRepository assistants)
        : base(logger, unitOfWork)
    {
        _projects = projects;
        _assistants = assistants;
    }

    protected override async Task<ProjectResearchAssistantDto> HandleInternal(
        AddProjectResearchAssistantCommand request,
        CancellationToken cancellationToken)
    {
        var project = await _projects.FindByIdAsync(request.ProjectId, cancellationToken)
            ?? throw new KeyNotFoundException($"Project {request.ProjectId} was not found.");

        ProjectResearchAssistantAccess.EnsureCanManage(project, request.CurrentUserId, request.IsAdmin);

        var email = request.Email!.Trim().ToLowerInvariant();

        if (string.Equals(project.EmailSnapshot.Trim(), email, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "The faculty member who posted this project cannot also be its research assistant.");
        }

        var existing = await _assistants.ListByProjectAsync(project.Id, cancellationToken);
        if (existing.Any(assistant => assistant.Email == email))
        {
            throw new InvalidOperationException("This person is already a research assistant on this project.");
        }

        var assistant = new ProjectResearchAssistant
        {
            ProjectId = project.Id,
            Email = email,
            AddedByUserId = request.CurrentUserId,
            CreatedAt = DateTime.UtcNow,
        };

        _assistants.Add(assistant);
        await UnitOfWork.SaveChangesAsync(cancellationToken);

        var names = await _assistants.NamesForEmailsAsync([email], cancellationToken);
        return assistant.ToDto(names);
    }
}
