using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using FEA.URVP.Application.Projects;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Projects.ResearchAssistants.Remove;

public sealed class RemoveProjectResearchAssistantCommandHandler
    : BaseCommandHandler<RemoveProjectResearchAssistantCommand>
{
    private readonly IProjectRepository _projects;
    private readonly IProjectResearchAssistantRepository _assistants;

    public RemoveProjectResearchAssistantCommandHandler(
        ILogger<RemoveProjectResearchAssistantCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IProjectRepository projects,
        IProjectResearchAssistantRepository assistants)
        : base(logger, unitOfWork)
    {
        _projects = projects;
        _assistants = assistants;
    }

    protected override async Task HandleCommandAsync(
        RemoveProjectResearchAssistantCommand request,
        CancellationToken cancellationToken)
    {
        var project = await _projects.FindByIdAsync(request.ProjectId, cancellationToken)
            ?? throw new KeyNotFoundException($"Project {request.ProjectId} was not found.");

        ProjectResearchAssistantAccess.EnsureCanManage(project, request.CurrentUserId, request.IsAdmin);

        var assistant = await _assistants.FindTrackedAsync(
                project.Id,
                request.AssistantId,
                cancellationToken)
            ?? throw new KeyNotFoundException($"Research assistant {request.AssistantId} was not found.");

        _assistants.Remove(assistant);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
    }
}
