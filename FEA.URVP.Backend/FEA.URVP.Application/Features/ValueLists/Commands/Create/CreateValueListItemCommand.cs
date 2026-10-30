using System.Text.Json.Serialization;
using FEA.URVP.Application.Features.ValueLists.DTOs;
using FEA.URVP.Domain.Enums;
using MediatR;

namespace FEA.URVP.Application.Features.ValueLists.Commands.Create;

public sealed class CreateValueListItemCommand : IRequest<ValueListItemDto>
{
    [JsonIgnore]
    public ValueListKind Kind { get; set; }

    public string Name { get; init; } = null!;
    public bool IsActive { get; init; } = true;
}
