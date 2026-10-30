using FEA.URVP.Domain.Enums;
using MediatR;

namespace FEA.URVP.Application.Features.ValueLists.Commands.Delete;

public sealed class DeleteValueListItemCommand : IRequest<Unit>
{
    public Guid Id { get; }
    public ValueListKind Kind { get; }

    public DeleteValueListItemCommand(Guid id, ValueListKind kind)
    {
        Id = id;
        Kind = kind;
    }
}
