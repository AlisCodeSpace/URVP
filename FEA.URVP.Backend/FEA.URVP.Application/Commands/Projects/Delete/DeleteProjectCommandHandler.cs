using FEA.URVP.Application.Abstractions.Events;
using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using FEA.URVP.Application.Notifications;
using FEA.URVP.Application.Projects;
using FEA.URVP.Domain.Events.Projects;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Projects.Delete;

public sealed class DeleteProjectCommandHandler : BaseCommandHandler<DeleteProjectCommand>
{
    private readonly IProjectRepository _projects;
    private readonly IEventBus _eventBus;
    private readonly FacultyProjectMutationAccess _mutationAccess;

    public DeleteProjectCommandHandler(
        ILogger<DeleteProjectCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IProjectRepository projects,
        IEventBus eventBus,
        FacultyProjectMutationAccess mutationAccess)
        : base(logger, unitOfWork)
    {
        _projects = projects;
        _eventBus = eventBus;
        _mutationAccess = mutationAccess;
    }

    protected override async Task HandleCommandAsync(
        DeleteProjectCommand request,
        CancellationToken cancellationToken)
    {
        var project = await _projects.FindByIdAsync(request.ProjectId, cancellationToken)
            ?? throw new KeyNotFoundException($"Project {request.ProjectId} was not found.");

        if (!request.IsAdmin && project.CreatedByUserId != request.CurrentUserId)
        {
            throw new UnauthorizedAccessException("You can only delete your own projects.");
        }

        await _mutationAccess.EnsureCanEditProjectAsync(
            project,
            request.IsAdmin,
            cancellationToken);

        var deletedEvent = request.IsAdmin && project.CreatedByUserId != request.CurrentUserId
            ? new ProjectDeletedEvent(project.Id, project.CreatedByUserId, project.Title)
            : null;

        _projects.Remove(project);
        await UnitOfWork.SaveChangesAsync(cancellationToken);

        Logger.LogInformation("Deleted project {ProjectId}", project.Id);

        if (deletedEvent is not null)
        {
            await NotificationEventPublish.TryPublishAsync(
                _eventBus,
                deletedEvent,
                Logger,
                cancellationToken);
        }
    }
}
