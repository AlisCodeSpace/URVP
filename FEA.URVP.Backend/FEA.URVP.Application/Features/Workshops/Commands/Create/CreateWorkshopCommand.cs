using FEA.URVP.Application.Features.Workshops.DTOs;
using MediatR;

namespace FEA.URVP.Application.Features.Workshops.Commands.Create;

public sealed class CreateWorkshopCommand : IRequest<WorkshopDto>
{
    public string Title { get; init; } = null!;
    public string Date { get; init; } = null!;
    public string? Time { get; init; }
    public string? Location { get; init; }
    public string Description { get; init; } = null!;
    public string RegistrationUrl { get; init; } = null!;
    public string? PosterAlt { get; init; }
    public bool Published { get; init; } = true;
}
