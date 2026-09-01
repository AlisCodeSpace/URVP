using MediatR;

namespace FEA.URVP.Application.Commands.News.Delete;

public sealed class DeleteNewsArticleCommand : IRequest<Unit>
{
    public Guid Id { get; }

    public DeleteNewsArticleCommand(Guid id)
    {
        Id = id;
    }
}
