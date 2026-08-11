using MediatR;

namespace FEA.URVP.Application.Commands.Divisions.Delete;

public sealed class DeleteDivisionCommand : IRequest<Unit>
{
    public Guid Id { get; }

    public DeleteDivisionCommand(Guid id)
    {
        Id = id;
    }
}
