using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using FEA.URVP.Application.DTOs.Projects;
using FEA.URVP.Application.Mappings;
using FEA.URVP.Domain.Entities.Projects;
using FEA.URVP.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Projects.Create;

public sealed class CreateProjectCommandHandler
    : BaseCommandHandler<CreateProjectCommand, ProjectDto>
{
    private readonly IProjectRepository _projects;
    private readonly IUserRepository _users;

    public CreateProjectCommandHandler(
        ILogger<CreateProjectCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IProjectRepository projects,
        IUserRepository users)
        : base(logger, unitOfWork)
    {
        _projects = projects;
        _users = users;
    }

    protected override async Task<ProjectDto> HandleInternal(
        CreateProjectCommand request,
        CancellationToken cancellationToken)
    {
        if (request.CurrentUserId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Authenticated user is required.");
        }

        var user = await _users.FindByIdAsync(request.CurrentUserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("User not found.");

        if (user.Role is not (UserRole.Faculty or UserRole.Admin))
        {
            throw new UnauthorizedAccessException("Only faculty or admins can create projects.");
        }

        var now = DateTime.UtcNow;
        var project = new Project
        {
            CreatedByUserId = user.Id,
            Title = request.Title.Trim(),
            ResearchAreas = request.ResearchAreas.ToList(),
            IrbStage = request.IrbStage,
            BriefDescription = request.BriefDescription.Trim(),
            ActivityTypes = request.ActivityTypes.ToList(),
            VolunteersRequired = request.VolunteersRequired,
            VolunteersFilled = 0,
            MinQualifications = NormalizeOptional(request.MinQualifications),
            AdditionalComments = NormalizeOptional(request.AdditionalComments),
            Status = ProjectStatus.Open,
            FacultyNameSnapshot = user.Name,
            AffiliationSnapshot = user.Affiliation,
            EmailSnapshot = user.Email,
            UserNameSnapshot = user.UserName,
            CreatedAt = now,
            UpdatedAt = now
        };

        _projects.Add(project);
        await UnitOfWork.SaveChangesAsync(cancellationToken);

        Logger.LogInformation(
            "Created project {ProjectId} by user {UserId}",
            project.Id,
            user.Id);

        return project.ToDto();
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
