using System.Text.Json.Serialization;
using FEA.URVP.Application.DTOs.Projects;
using FEA.URVP.Domain.Enums;
using MediatR;

namespace FEA.URVP.Application.Commands.Projects.Create;

public sealed class CreateProjectCommand : IRequest<ProjectDto>
{
    [JsonIgnore]
    public Guid CurrentUserId { get; set; }

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
}
