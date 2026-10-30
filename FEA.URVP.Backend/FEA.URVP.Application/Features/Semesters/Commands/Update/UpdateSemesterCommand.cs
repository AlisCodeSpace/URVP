using System.Text.Json.Serialization;
using FEA.URVP.Application.Features.Semesters.DTOs;
using MediatR;

namespace FEA.URVP.Application.Features.Semesters.Commands.Update;

public sealed class UpdateSemesterCommand : IRequest<SemesterDto>
{
    [JsonIgnore]
    public Guid Id { get; set; }

    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public DateTime? CycleStart { get; init; }
    public DateTime? CycleEnd { get; init; }
    public DateTime? ApplicationWindowStart { get; init; }
    public DateTime? ApplicationWindowEnd { get; init; }
    public DateTime? RegistrationWindowStart { get; init; }
    public DateTime? RegistrationWindowEnd { get; init; }
}
