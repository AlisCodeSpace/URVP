using System.Text.Json.Serialization;
using FEA.URVP.Application.DTOs.Projects;
using MediatR;

namespace FEA.URVP.Application.Commands.Projects.ResearchAssistants.Add;

/// <summary>
/// Adds a research assistant. Pass <see cref="UserId"/> when the person comes from a
/// user list, or <see cref="Name"/> and <see cref="Email"/> for a typed entry.
/// </summary>
public sealed class AddProjectResearchAssistantCommand : IRequest<ProjectResearchAssistantDto>
{
    [JsonIgnore]
    public Guid ProjectId { get; set; }

    [JsonIgnore]
    public Guid CurrentUserId { get; set; }

    [JsonIgnore]
    public bool IsAdmin { get; set; }

    public Guid? UserId { get; init; }

    public string? Name { get; init; }

    public string? Email { get; init; }
}
