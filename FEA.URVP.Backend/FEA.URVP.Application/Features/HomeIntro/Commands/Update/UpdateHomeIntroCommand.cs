using System.Text.Json.Serialization;
using FEA.URVP.Application.Features.HomeIntro.DTOs;
using MediatR;

namespace FEA.URVP.Application.Features.HomeIntro.Commands.Update;

public sealed class UpdateHomeIntroCommand : IRequest<HomeIntroDto>
{
    [JsonIgnore]
    public Guid UpdatedByUserId { get; set; }

    public string Headline { get; init; } = null!;

    public string Description { get; init; } = null!;

    public IReadOnlyList<string> KeyPoints { get; init; } = [];
}
