using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using FEA.URVP.Domain.Catalog;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.News.Delete;

public sealed class DeleteNewsArticleCommandHandler
    : BaseCommandHandler<DeleteNewsArticleCommand>
{
    private readonly INewsArticleRepository _news;
    private readonly IFileStorageRepository _files;

    public DeleteNewsArticleCommandHandler(
        ILogger<DeleteNewsArticleCommandHandler> logger,
        IUnitOfWork unitOfWork,
        INewsArticleRepository news,
        IFileStorageRepository files)
        : base(logger, unitOfWork)
    {
        _news = news;
        _files = files;
    }

    protected override async Task HandleCommandAsync(
        DeleteNewsArticleCommand request,
        CancellationToken cancellationToken)
    {
        var article = await _news.FindByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"News article {request.Id} was not found.");

        var files = await _files.ListActiveByEntityAsync(
            FileStorageCatalog.EntityNewsArticle,
            article.Id,
            cancellationToken);
        foreach (var file in files)
        {
            file.IsDeleted = true;
        }

        _news.Remove(article);
        await UnitOfWork.SaveChangesAsync(cancellationToken);

        Logger.LogInformation("Deleted news article {NewsId} ({Slug})", article.Id, article.Slug);
    }
}
