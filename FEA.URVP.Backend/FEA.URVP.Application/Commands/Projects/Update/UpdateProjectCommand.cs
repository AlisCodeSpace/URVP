using System.Text.Json.Serialization;
using FEA.URVP.Application.DTOs.Projects;
using FEA.URVP.Domain.Enums;
using MediatR;

namespace FEA.URVP.Application.Commands.Projects.Update;

public sealed class UpdateProjectCommand : IRequest<ProjectDto>
{
    [JsonIgnore]
    public Guid ProjectId { get; set; }

    [JsonIgnore]
    public Guid CurrentUserId { get; set; }

    [JsonIgnore]
    public bool IsAdmin { get; set; }

    public string Title { get; init; } = null!;
    public List<string> ResearchAreas { get; init; } = [];
    public IrbStage IrbStage { get; init; }
    public string BriefDescription { get; init; } = null!;
    public List<string> ActivityTypes { get; init; } = [];
    public int VolunteersRequired { get; init; }
    public string? MinQualifications { get; init; }
    public string? AdditionalComments { get; init; }
    public string? Affiliation { get; init; }
    public string? UserName { get; init; }
    public ProjectStatus Status { get; init; }
}
