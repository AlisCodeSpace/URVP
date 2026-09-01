using FEA.URVP.Application.DTOs.Semesters;
using MediatR;

namespace FEA.URVP.Application.Commands.Semesters.Create;

public sealed class CreateSemesterCommand : IRequest<SemesterDto>
{
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
}
