using FEA.URVP.Application.DTOs.Semesters;
using MediatR;

namespace FEA.URVP.Application.Commands.Semesters.Create;

public sealed class CreateSemesterCommand : IRequest<SemesterDto>
{
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public DateTime? CycleStart { get; init; }
    public DateTime? CycleEnd { get; init; }
    public DateTime? ApplicationWindowStart { get; init; }
    public DateTime? ApplicationWindowEnd { get; init; }
}
