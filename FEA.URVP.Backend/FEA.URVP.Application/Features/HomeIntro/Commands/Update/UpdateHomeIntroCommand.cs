using System.Text.Json.Serialization;
using FEA.URVP.Application.DTOs.HomeIntro;
using MediatR;

namespace FEA.URVP.Application.Commands.HomeIntro.Update;

public sealed class UpdateHomeIntroCommand : IRequest<HomeIntroDto>
{
    [JsonIgnore]
    public Guid UpdatedByUserId { get; set; }

    public string Headline { get; init; } = null!;

    public string Description { get; init; } = null!;

    public IReadOnlyList<string> KeyPoints { get; init; } = [];
}
