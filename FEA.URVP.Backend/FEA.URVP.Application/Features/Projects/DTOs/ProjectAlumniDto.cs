namespace FEA.URVP.Application.DTOs.Projects;

public sealed class ProjectAlumniDto
{
    public Guid SemesterId { get; init; }
    public string SemesterName { get; init; } = null!;
    public Guid StudentUserId { get; init; }
    public string StudentName { get; init; } = null!;
    public string StudentEmail { get; init; } = null!;
    public byte? StudentRank { get; init; }
    public byte? FacultyRank { get; init; }
    public bool WasConfirmed { get; init; }
    public DateTime ArchivedAt { get; init; }
}
