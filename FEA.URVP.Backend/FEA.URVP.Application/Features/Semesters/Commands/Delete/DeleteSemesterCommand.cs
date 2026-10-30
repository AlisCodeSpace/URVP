using MediatR;

namespace FEA.URVP.Application.Features.Semesters.Commands.Delete;

public sealed class DeleteSemesterCommand : IRequest<Unit>
{
    public Guid Id { get; init; }
}
