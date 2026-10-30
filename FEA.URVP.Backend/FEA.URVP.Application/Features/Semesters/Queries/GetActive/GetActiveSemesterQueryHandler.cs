using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Features.Semesters.DTOs;
using FEA.URVP.Application.Features.Semesters.Mappings;
using MediatR;

namespace FEA.URVP.Application.Features.Semesters.Queries.GetActive;

public sealed class GetActiveSemesterQueryHandler
    : IRequestHandler<GetActiveSemesterQuery, SemesterDto?>
{
    private readonly ISemesterRepository _semesters;

    public GetActiveSemesterQueryHandler(ISemesterRepository semesters)
    {
        _semesters = semesters;
    }

    public async Task<SemesterDto?> Handle(
        GetActiveSemesterQuery request,
        CancellationToken cancellationToken)
    {
        var semester = await _semesters.FindActiveAsync(cancellationToken);
        return semester?.ToDto();
    }
}
