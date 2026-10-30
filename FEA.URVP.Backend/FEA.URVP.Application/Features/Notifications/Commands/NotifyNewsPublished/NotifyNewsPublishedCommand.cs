using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Notifications.Create;
using FEA.URVP.Application.Notifications;
using FEA.URVP.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Notifications.NotifyNewsPublished;

public sealed record NotifyNewsPublishedCommand(Guid ArticleId) : IRequest<int>;

public sealed class NotifyNewsPublishedCommandHandler : IRequestHandler<NotifyNewsPublishedCommand, int>
{
    public const string ReferenceType = "NewsArticle";

    private readonly INewsArticleRepository _news;
    private readonly IUserRepository _users;
    private readonly IMediator _mediator;
    private readonly ILogger<NotifyNewsPublishedCommandHandler> _logger;

    public NotifyNewsPublishedCommandHandler(
        INewsArticleRepository news,
        IUserRepository users,
        IMediator mediator,
        ILogger<NotifyNewsPublishedCommandHandler> logger)
    {
        _news = news;
        _users = users;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<int> Handle(NotifyNewsPublishedCommand request, CancellationToken cancellationToken)
    {
        var article = await _news.FindByIdAsync(request.ArticleId, cancellationToken)
            ?? throw new KeyNotFoundException($"News article {request.ArticleId} was not found.");

        var recipients = await _users.ListUserIdsByRolesAsync(
            [UserRole.Student, UserRole.Faculty],
            cancellationToken);

        return await NotificationFanOut.SendAsync(
            _mediator,
            _logger,
            recipients,
            userId => new CreateNotificationCommand(
                userId,
                NotificationMessages.News.NewsPublishedTitle(),
                NotificationMessages.News.NewsPublishedMessage(article.Title),
                NotificationType.NewsPublished,
                NotificationLinks.NewsArticle(article.Slug),
                NotificationPriority.Low,
                article.Id,
                ReferenceType),
            cancellationToken);
    }
}
