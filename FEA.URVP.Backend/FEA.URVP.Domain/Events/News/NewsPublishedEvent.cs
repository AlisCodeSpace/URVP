using FEA.URVP.Domain.Events;

namespace FEA.URVP.Domain.Events.News;

public sealed class NewsPublishedEvent : DomainEvent
{
    public NewsPublishedEvent(Guid articleId)
    {
        ArticleId = articleId;
    }

    public Guid ArticleId { get; }
}
