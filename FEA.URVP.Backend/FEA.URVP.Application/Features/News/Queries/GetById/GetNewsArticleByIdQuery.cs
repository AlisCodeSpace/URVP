using FEA.URVP.Application.Features.News.DTOs;
using MediatR;

namespace FEA.URVP.Application.Features.News.Queries.GetById;

public sealed class GetNewsArticleByIdQuery : IRequest<NewsArticleDto>
{
    public Guid Id { get; }

    public GetNewsArticleByIdQuery(Guid id)
    {
        Id = id;
    }
}
