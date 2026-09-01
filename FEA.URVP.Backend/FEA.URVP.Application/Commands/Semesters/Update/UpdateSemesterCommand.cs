using System.Text.Json.Serialization;
using FEA.URVP.Application.DTOs.Semesters;
using MediatR;

namespace FEA.URVP.Application.Commands.Semesters.Update;

public sealed class UpdateSemesterCommand : IRequest<SemesterDto>
{
    [JsonIgnore]
    public Guid Id { get; set; }

    public string Name { get; init; } = null!;
    public string? Description { get; init; }
}
