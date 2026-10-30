using System.Text.Json.Serialization;
using FEA.URVP.Application.Features.ValueLists.DTOs;
using FEA.URVP.Domain.Enums;
using MediatR;

namespace FEA.URVP.Application.Features.ValueLists.Commands.Update;

public sealed class UpdateValueListItemCommand : IRequest<ValueListItemDto>
{
    [JsonIgnore]
    public Guid Id { get; set; }

    [JsonIgnore]
    public ValueListKind Kind { get; set; }

    public string Name { get; init; } = null!;
    public bool? IsActive { get; init; }
}
