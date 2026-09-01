using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using FEA.URVP.Application.DTOs.News;
using FEA.URVP.Application.Mappings;
using FEA.URVP.Domain.Catalog;
using FEA.URVP.Domain.Entities.News;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.News.Create;

public sealed class CreateNewsArticleCommandHandler
    : BaseCommandHandler<CreateNewsArticleCommand, NewsArticleDto>
{
    private readonly INewsArticleRepository _news;

    public CreateNewsArticleCommandHandler(
        ILogger<CreateNewsArticleCommandHandler> logger,
        IUnitOfWork unitOfWork,
        INewsArticleRepository news)
        : base(logger, unitOfWork)
    {
        _news = news;
    }

    protected override async Task<NewsArticleDto> HandleInternal(
        CreateNewsArticleCommand request,
        CancellationToken cancellationToken)
    {
        var slug = await EnsureUniqueSlugAsync(
            string.IsNullOrWhiteSpace(request.Slug)
                ? ContentSlug.FromTitle(request.Title)
                : ContentSlug.FromTitle(request.Slug),
            exceptId: null,
            cancellationToken);

        if (request.Featured)
        {
            await _news.ClearFeaturedAsync(null, cancellationToken);
        }

        var now = DateTime.UtcNow;
        var article = new NewsArticle
        {
            Slug = slug,
            Title = request.Title.Trim(),
            Excerpt = request.Excerpt.Trim(),
            Category = request.Category.Trim(),
            Author = request.Author.Trim(),
            Ticker = request.Ticker.Trim(),
            Body = request.Body.Select(p => p.Trim()).Where(p => p.Length > 0).ToList(),
            PublishedAt = DateTime.SpecifyKind(request.PublishedAt.Date, DateTimeKind.Utc),
            Featured = request.Featured,
            CreatedAt = now,
            UpdatedAt = now,
        };

        _news.Add(article);
        await UnitOfWork.SaveChangesAsync(cancellationToken);

        Logger.LogInformation("Created news article {NewsId} ({Slug})", article.Id, article.Slug);

        return article.ToDto();
    }

    private async Task<string> EnsureUniqueSlugAsync(
        string slug,
        Guid? exceptId,
        CancellationToken cancellationToken)
    {
        var candidate = slug;
        var suffix = 2;
        while (true)
        {
            var existing = await _news.FindBySlugAsync(candidate, cancellationToken);
            if (existing is null || existing.Id == exceptId)
            {
                return candidate;
            }

            candidate = $"{slug}-{suffix}";
            suffix++;
        }
    }
}
