using System.Text.Json.Serialization;
using FEA.URVP.Application.DTOs.ValueLists;
using FEA.URVP.Domain.Enums;
using MediatR;

namespace FEA.URVP.Application.Commands.ValueLists.Create;

public sealed class CreateValueListItemCommand : IRequest<ValueListItemDto>
{
    [JsonIgnore]
    public ValueListKind Kind { get; set; }

    public string Name { get; init; } = null!;
    public bool IsActive { get; init; } = true;
}
