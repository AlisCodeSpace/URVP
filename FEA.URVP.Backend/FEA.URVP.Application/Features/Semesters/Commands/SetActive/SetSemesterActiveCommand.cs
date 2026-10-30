using FEA.URVP.Application.Features.Semesters.DTOs;
using MediatR;

namespace FEA.URVP.Application.Features.Semesters.Commands.SetActive;

/// <summary>
/// Activates or deactivates a semester cycle.
/// Activating deactivates all other semesters automatically.
/// </summary>
public sealed class SetSemesterActiveCommand : IRequest<SemesterDto>
{
    public Guid Id { get; init; }

    /// <summary>True to start the cycle; false to end it.</summary>
    public bool IsActive { get; init; }
}
