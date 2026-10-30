using FEA.URVP.Application.DTOs.News;
using MediatR;

namespace FEA.URVP.Application.Queries.News.GetById;

public sealed class GetNewsArticleByIdQuery : IRequest<NewsArticleDto>
{
    public Guid Id { get; }

    public GetNewsArticleByIdQuery(Guid id)
    {
        Id = id;
    }
}
