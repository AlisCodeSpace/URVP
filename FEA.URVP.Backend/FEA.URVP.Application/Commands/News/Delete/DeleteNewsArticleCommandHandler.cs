using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.News.Delete;

public sealed class DeleteNewsArticleCommandHandler
    : BaseCommandHandler<DeleteNewsArticleCommand>
{
    private readonly INewsArticleRepository _news;

    public DeleteNewsArticleCommandHandler(
        ILogger<DeleteNewsArticleCommandHandler> logger,
        IUnitOfWork unitOfWork,
        INewsArticleRepository news)
        : base(logger, unitOfWork)
    {
        _news = news;
    }

    protected override async Task HandleCommandAsync(
        DeleteNewsArticleCommand request,
        CancellationToken cancellationToken)
    {
        var article = await _news.FindByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"News article {request.Id} was not found.");

        _news.Remove(article);
        await UnitOfWork.SaveChangesAsync(cancellationToken);

        Logger.LogInformation("Deleted news article {NewsId} ({Slug})", article.Id, article.Slug);
    }
}
