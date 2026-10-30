using MediatR;

namespace FEA.URVP.Application.Commands.Semesters.Delete;

public sealed class DeleteSemesterCommand : IRequest<Unit>
{
    public Guid Id { get; init; }
}
