using FEA.URVP.Application.Abstractions.Events;
using FEA.URVP.Application.Features.Notifications.Commands.NotifyNewsPublished;
using FEA.URVP.Domain.Events.News;
using MediatR;

namespace FEA.URVP.Application.Features.News.Events;

public sealed class NewsPublishedEventHandler : IEventHandler<NewsPublishedEvent>
{
    private readonly IMediator _mediator;

    public NewsPublishedEventHandler(IMediator mediator) => _mediator = mediator;

    public Task HandleAsync(NewsPublishedEvent domainEvent, CancellationToken cancellationToken = default) =>
        _mediator.Send(new NotifyNewsPublishedCommand(domainEvent.ArticleId), cancellationToken);
}
