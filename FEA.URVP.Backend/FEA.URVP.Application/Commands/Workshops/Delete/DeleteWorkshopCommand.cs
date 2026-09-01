using MediatR;

namespace FEA.URVP.Application.Commands.Workshops.Delete;

public sealed class DeleteWorkshopCommand : IRequest<Unit>
{
    public Guid Id { get; }

    public DeleteWorkshopCommand(Guid id)
    {
        Id = id;
    }
}
