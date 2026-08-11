using System.Text.Json.Serialization;
using FEA.URVP.Application.DTOs.Divisions;
using MediatR;

namespace FEA.URVP.Application.Commands.Divisions.Update;

public sealed class UpdateDivisionCommand : IRequest<DivisionDto>
{
    [JsonIgnore]
    public Guid Id { get; set; }

    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public bool? IsActive { get; init; }
}
