using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using FEA.URVP.Application.DTOs.News;
using FEA.URVP.Application.Mappings;
using FEA.URVP.Domain.Catalog;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.News.Update;

public sealed class UpdateNewsArticleCommandHandler
    : BaseCommandHandler<UpdateNewsArticleCommand, NewsArticleDto>
{
    private readonly INewsArticleRepository _news;

    public UpdateNewsArticleCommandHandler(
        ILogger<UpdateNewsArticleCommandHandler> logger,
        IUnitOfWork unitOfWork,
        INewsArticleRepository news)
        : base(logger, unitOfWork)
    {
        _news = news;
    }

    protected override async Task<NewsArticleDto> HandleInternal(
        UpdateNewsArticleCommand request,
        CancellationToken cancellationToken)
    {
        var article = await _news.FindByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"News article {request.Id} was not found.");

        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? ContentSlug.FromTitle(request.Title)
            : ContentSlug.FromTitle(request.Slug);

        var duplicate = await _news.FindBySlugAsync(slug, cancellationToken);
        if (duplicate is not null && duplicate.Id != article.Id)
        {
            throw new InvalidOperationException($"A news article with slug \"{slug}\" already exists.");
        }

        if (request.Featured)
        {
            await _news.ClearFeaturedAsync(article.Id, cancellationToken);
        }

        article.Slug = slug;
        article.Title = request.Title.Trim();
        article.Excerpt = request.Excerpt.Trim();
        article.Category = request.Category.Trim();
        article.Author = request.Author.Trim();
        article.Ticker = request.Ticker.Trim();
        article.Body = request.Body.Select(p => p.Trim()).Where(p => p.Length > 0).ToList();
        article.PublishedAt = DateTime.SpecifyKind(request.PublishedAt.Date, DateTimeKind.Utc);
        article.Featured = request.Featured;
        article.UpdatedAt = DateTime.UtcNow;

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        Logger.LogInformation("Updated news article {NewsId}", article.Id);

        return article.ToDto();
    }
}
