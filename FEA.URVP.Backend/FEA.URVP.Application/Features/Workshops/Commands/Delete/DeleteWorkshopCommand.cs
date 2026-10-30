using MediatR;

namespace FEA.URVP.Application.Features.Workshops.Commands.Delete;

public sealed class DeleteWorkshopCommand : IRequest<Unit>
{
    public Guid Id { get; }

    public DeleteWorkshopCommand(Guid id)
    {
        Id = id;
    }
}
