using System.Text.Json.Serialization;
using FEA.URVP.Application.DTOs.Projects;
using MediatR;

namespace FEA.URVP.Application.Commands.Projects.ResearchAssistants.Add;

/// <summary>
/// Adds a research assistant by email. The person need not have signed in yet; they are
/// matched by email whenever they do.
/// </summary>
public sealed class AddProjectResearchAssistantCommand : IRequest<ProjectResearchAssistantDto>
{
    [JsonIgnore]
    public Guid ProjectId { get; set; }

    [JsonIgnore]
    public Guid CurrentUserId { get; set; }

    [JsonIgnore]
    public bool IsAdmin { get; set; }

    public string? Email { get; init; }
}
