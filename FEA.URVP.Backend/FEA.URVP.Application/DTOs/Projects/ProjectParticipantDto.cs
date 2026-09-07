using FEA.URVP.Domain.Enums;

namespace FEA.URVP.Application.DTOs.Projects;

/// <summary>A student confirmed onto a project.</summary>
public sealed class ProjectParticipantDto
{
    public Guid PlacementId { get; init; }
    public Guid StudentUserId { get; init; }
    public string StudentName { get; init; } = null!;
    public string StudentEmail { get; init; } = null!;
    public byte StudentRank { get; init; }
    public byte FacultyRank { get; init; }
    public PlacementSource Source { get; init; }
}
