using FEA.URVP.Application.DTOs.Divisions;
using MediatR;

namespace FEA.URVP.Application.Commands.Divisions.Create;

public sealed class CreateDivisionCommand : IRequest<DivisionDto>
{
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public bool IsActive { get; init; } = true;
}
