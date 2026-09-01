using FEA.URVP.Application.DTOs.News;
using MediatR;

namespace FEA.URVP.Application.Queries.News.GetBySlug;

public sealed class GetNewsArticleBySlugQuery : IRequest<NewsArticleDto>
{
    public string Slug { get; }

    public GetNewsArticleBySlugQuery(string slug)
    {
        Slug = slug;
    }
}
