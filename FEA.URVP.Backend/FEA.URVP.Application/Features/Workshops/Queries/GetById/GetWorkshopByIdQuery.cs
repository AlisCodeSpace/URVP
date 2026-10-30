using FEA.URVP.Application.Features.Workshops.DTOs;
using MediatR;

namespace FEA.URVP.Application.Features.Workshops.Queries.GetById;

public sealed class GetWorkshopByIdQuery : IRequest<WorkshopDto>
{
    public Guid Id { get; }
    public bool PublishedOnly { get; }

    public GetWorkshopByIdQuery(Guid id, bool publishedOnly = false)
    {
        Id = id;
        PublishedOnly = publishedOnly;
    }
}
