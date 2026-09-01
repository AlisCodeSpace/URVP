using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.DTOs.Workshops;
using FEA.URVP.Application.Mappings;
using MediatR;

namespace FEA.URVP.Application.Queries.Workshops.GetById;

public sealed class GetWorkshopByIdQueryHandler
    : IRequestHandler<GetWorkshopByIdQuery, WorkshopDto>
{
    private readonly IWorkshopRepository _workshops;

    public GetWorkshopByIdQueryHandler(IWorkshopRepository workshops)
    {
        _workshops = workshops;
    }

    public async Task<WorkshopDto> Handle(
        GetWorkshopByIdQuery request,
        CancellationToken cancellationToken)
    {
        var workshop = await _workshops.FindByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Workshop {request.Id} was not found.");

        return workshop.ToDto();
    }
}
