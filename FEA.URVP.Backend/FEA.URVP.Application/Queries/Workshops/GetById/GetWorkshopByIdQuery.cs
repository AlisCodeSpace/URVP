using FEA.URVP.Application.DTOs.Workshops;
using MediatR;

namespace FEA.URVP.Application.Queries.Workshops.GetById;

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
