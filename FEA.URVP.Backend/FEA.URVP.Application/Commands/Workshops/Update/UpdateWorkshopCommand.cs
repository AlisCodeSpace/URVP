using System.Text.Json.Serialization;
using FEA.URVP.Application.DTOs.Workshops;
using MediatR;

namespace FEA.URVP.Application.Commands.Workshops.Update;

public sealed class UpdateWorkshopCommand : IRequest<WorkshopDto>
{
    [JsonIgnore]
    public Guid Id { get; set; }

    public string Title { get; init; } = null!;
    public string Date { get; init; } = null!;
    public string? Time { get; init; }
    public string? Location { get; init; }
    public string Description { get; init; } = null!;
    public string RegistrationUrl { get; init; } = null!;
    public Guid? PosterFileId { get; init; }
    public string? PosterAlt { get; init; }
}
