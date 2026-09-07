using System.Text.Json.Serialization;
using FEA.URVP.Application.DTOs.Matching;
using MediatR;

namespace FEA.URVP.Application.Commands.Matching.Assign;

/// <summary>
/// Assigns a student to a project immediately. Occupies a seat and notifies the student and faculty.
/// </summary>
public sealed class AssignStudentToProjectCommand : IRequest<PlacementDto>
{
    [JsonIgnore]
    public Guid CurrentUserId { get; set; }

    public Guid ProjectId { get; init; }

    public Guid StudentUserId { get; init; }
}
