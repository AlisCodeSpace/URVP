using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Projects.Delete;

public sealed class DeleteProjectCommandHandler : BaseCommandHandler<DeleteProjectCommand>
{
    private readonly IProjectRepository _projects;

    public DeleteProjectCommandHandler(
        ILogger<DeleteProjectCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IProjectRepository projects)
        : base(logger, unitOfWork)
    {
        _projects = projects;
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

        _projects.Remove(project);
        await UnitOfWork.SaveChangesAsync(cancellationToken);

        Logger.LogInformation("Deleted project {ProjectId}", project.Id);
    }
}
