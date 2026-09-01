using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.DTOs.News;
using FEA.URVP.Application.Mappings;
using MediatR;

namespace FEA.URVP.Application.Queries.News.GetBySlug;

public sealed class GetNewsArticleBySlugQueryHandler
    : IRequestHandler<GetNewsArticleBySlugQuery, NewsArticleDto>
{
    private readonly INewsArticleRepository _news;

    public GetNewsArticleBySlugQueryHandler(INewsArticleRepository news)
    {
        _news = news;
    }

    public async Task<NewsArticleDto> Handle(
        GetNewsArticleBySlugQuery request,
        CancellationToken cancellationToken)
    {
        var article = await _news.FindBySlugAsync(request.Slug, cancellationToken)
            ?? throw new KeyNotFoundException($"News article '{request.Slug}' was not found.");

        return article.ToDto();
    }
}
