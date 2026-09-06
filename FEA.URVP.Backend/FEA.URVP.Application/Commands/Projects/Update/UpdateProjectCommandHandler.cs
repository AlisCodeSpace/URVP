using FEA.URVP.Application.Abstractions.Events;
using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using FEA.URVP.Application.DTOs.Projects;
using FEA.URVP.Application.Mappings;
using FEA.URVP.Application.Notifications;
using FEA.URVP.Domain.Enums;
using FEA.URVP.Domain.Events.Projects;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Projects.Update;

public sealed class UpdateProjectCommandHandler
    : BaseCommandHandler<UpdateProjectCommand, ProjectDto>
{
    private readonly IProjectRepository _projects;
    private readonly IUserRepository _users;
    private readonly IEventBus _eventBus;

    public UpdateProjectCommandHandler(
        ILogger<UpdateProjectCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IProjectRepository projects,
        IUserRepository users,
        IEventBus eventBus)
        : base(logger, unitOfWork)
    {
        _projects = projects;
        _users = users;
        _eventBus = eventBus;
    }

    protected override async Task<ProjectDto> HandleInternal(
        UpdateProjectCommand request,
        CancellationToken cancellationToken)
    {
        var project = await _projects.FindByIdAsync(request.ProjectId, cancellationToken)
            ?? throw new KeyNotFoundException($"Project {request.ProjectId} was not found.");

        if (!request.IsAdmin && project.CreatedByUserId != request.CurrentUserId)
        {
            throw new UnauthorizedAccessException("You can only update your own projects.");
        }

        if (request.VolunteersRequired < project.VolunteersFilled)
        {
            throw new ArgumentException(
                $"Volunteers required cannot be less than already filled ({project.VolunteersFilled}).");
        }

        var owner = await _users.FindByIdAsync(project.CreatedByUserId, cancellationToken)
            ?? throw new InvalidOperationException("Project owner was not found.");

        var previousStatus = project.Status;

        project.Title = request.Title.Trim();
        project.ResearchAreas = request.ResearchAreas.ToList();
        project.IrbStage = request.IrbStage;
        project.BriefDescription = request.BriefDescription.Trim();
        project.ActivityTypes = request.ActivityTypes.ToList();
        project.VolunteersRequired = request.VolunteersRequired;
        project.MinQualifications = NormalizeOptional(request.MinQualifications);
        project.AdditionalComments = NormalizeOptional(request.AdditionalComments);
        project.FacultyNameSnapshot = owner.Name;
        project.AffiliationSnapshot = owner.Affiliation;
        project.EmailSnapshot = owner.Email;
        project.UserNameSnapshot = owner.UserName;
        project.Status = request.Status;
        project.UpdatedAt = DateTime.UtcNow;

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        Logger.LogInformation("Updated project {ProjectId}", project.Id);

        if (previousStatus != ProjectStatus.Closed && project.Status == ProjectStatus.Closed)
        {
            await NotificationEventPublish.TryPublishAsync(
                _eventBus,
                new ProjectClosedEvent(
                    project.Id,
                    project.CreatedByUserId,
                    notifyOwner: request.IsAdmin && project.CreatedByUserId != request.CurrentUserId),
                Logger,
                cancellationToken);
        }

        return project.ToDto();
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
