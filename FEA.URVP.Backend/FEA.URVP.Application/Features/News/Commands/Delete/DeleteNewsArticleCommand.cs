using MediatR;

namespace FEA.URVP.Application.Features.News.Commands.Delete;

public sealed class DeleteNewsArticleCommand : IRequest<Unit>
{
    public Guid Id { get; }

    public DeleteNewsArticleCommand(Guid id)
    {
        Id = id;
    }
}
