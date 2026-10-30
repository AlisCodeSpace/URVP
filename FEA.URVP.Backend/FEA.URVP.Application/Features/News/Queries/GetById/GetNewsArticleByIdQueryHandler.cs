using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.DTOs.News;
using FEA.URVP.Application.Mappings;
using MediatR;

namespace FEA.URVP.Application.Queries.News.GetById;

public sealed class GetNewsArticleByIdQueryHandler
    : IRequestHandler<GetNewsArticleByIdQuery, NewsArticleDto>
{
    private readonly INewsArticleRepository _news;

    public GetNewsArticleByIdQueryHandler(INewsArticleRepository news)
    {
        _news = news;
    }

    public async Task<NewsArticleDto> Handle(
        GetNewsArticleByIdQuery request,
        CancellationToken cancellationToken)
    {
        var article = await _news.FindByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"News article {request.Id} was not found.");

        return article.ToDto();
    }
}
