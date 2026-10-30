using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Features.News.DTOs;
using FEA.URVP.Application.Features.News.Mappings;
using MediatR;

namespace FEA.URVP.Application.Features.News.Queries.GetById;

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
