using System.Text.Json.Serialization;
using FEA.URVP.Application.DTOs.Semesters;
using MediatR;

namespace FEA.URVP.Application.Commands.Semesters.SetApplicationWindow;

/// <summary>
/// Opens or closes the student application window for a semester.
/// Pass null for ApplicationWindowStart to reset the window entirely.
/// Pass null for ApplicationWindowEnd to leave the window open indefinitely.
/// </summary>
public sealed class SetApplicationWindowCommand : IRequest<SemesterDto>
{
    [JsonIgnore]
    public Guid Id { get; set; }

    /// <summary>
    /// UTC start of the application window. Pass null to clear (window not yet opened).
    /// </summary>
    public DateTime? ApplicationWindowStart { get; init; }

    /// <summary>
    /// UTC end of the application window. Pass null to leave the window open.
    /// </summary>
    public DateTime? ApplicationWindowEnd { get; init; }
}
