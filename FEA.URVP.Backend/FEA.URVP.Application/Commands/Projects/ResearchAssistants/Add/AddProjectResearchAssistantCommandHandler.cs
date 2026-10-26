using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using FEA.URVP.Application.DTOs.Projects;
using FEA.URVP.Application.Projects;
using FEA.URVP.Domain.Entities.Projects;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Projects.ResearchAssistants.Add;

public sealed class AddProjectResearchAssistantCommandHandler
    : BaseCommandHandler<AddProjectResearchAssistantCommand, ProjectResearchAssistantDto>
{
    private readonly IProjectRepository _projects;
    private readonly IUserRepository _users;
    private readonly IProjectResearchAssistantRepository _assistants;

    public AddProjectResearchAssistantCommandHandler(
        ILogger<AddProjectResearchAssistantCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IProjectRepository projects,
        IUserRepository users,
        IProjectResearchAssistantRepository assistants)
        : base(logger, unitOfWork)
    {
        _projects = projects;
        _users = users;
        _assistants = assistants;
    }

    protected override async Task<ProjectResearchAssistantDto> HandleInternal(
        AddProjectResearchAssistantCommand request,
        CancellationToken cancellationToken)
    {
        var project = await _projects.FindByIdAsync(request.ProjectId, cancellationToken)
            ?? throw new KeyNotFoundException($"Project {request.ProjectId} was not found.");

        ProjectResearchAssistantAccess.EnsureCanManage(project, request.CurrentUserId, request.IsAdmin);

        var existing = await _assistants.ListByProjectAsync(project.Id, cancellationToken);
        var userId = request.UserId is { } id && id != Guid.Empty ? id : (Guid?)null;

        string name;
        string email;
        if (userId is Guid linkedUserId)
        {
            var user = await _users.FindByIdAsync(linkedUserId, cancellationToken)
                ?? throw new KeyNotFoundException($"User {linkedUserId} was not found.");

            if (existing.Any(assistant => assistant.UserId == user.Id))
            {
                throw new InvalidOperationException("This person is already a research assistant on this project.");
            }

            name = user.Name.Trim();
            email = user.Email.Trim();
        }
        else
        {
            name = request.Name!.Trim();
            email = request.Email!.Trim();
        }

        if (existing.Any(assistant => EmailsMatch(assistant.Email, email)))
        {
            throw new InvalidOperationException("This person is already a research assistant on this project.");
        }

        var assistant = new ProjectResearchAssistant
        {
            ProjectId = project.Id,
            UserId = userId,
            Name = name,
            Email = email,
            AddedByUserId = request.CurrentUserId,
            CreatedAt = DateTime.UtcNow,
        };

        _assistants.Add(assistant);
        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(assistant);
    }

    internal static bool EmailsMatch(string left, string right) =>
        string.Equals(left.Trim(), right.Trim(), StringComparison.OrdinalIgnoreCase);

    internal static ProjectResearchAssistantDto ToDto(ProjectResearchAssistant assistant) =>
        new()
        {
            Id = assistant.Id,
            UserId = assistant.UserId,
            Name = assistant.Name,
            Email = assistant.Email,
            AddedAt = assistant.CreatedAt,
        };
}
