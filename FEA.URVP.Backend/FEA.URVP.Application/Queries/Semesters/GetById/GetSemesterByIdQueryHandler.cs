using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.DTOs.Semesters;
using FEA.URVP.Application.Mappings;
using MediatR;

namespace FEA.URVP.Application.Queries.Semesters.GetById;

public sealed class GetSemesterByIdQueryHandler
    : IRequestHandler<GetSemesterByIdQuery, SemesterDto>
{
    private readonly ISemesterRepository _semesters;

    public GetSemesterByIdQueryHandler(ISemesterRepository semesters)
    {
        _semesters = semesters;
    }

    public async Task<SemesterDto> Handle(
        GetSemesterByIdQuery request,
        CancellationToken cancellationToken)
    {
        var semester = await _semesters.FindByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Semester {request.Id} was not found.");

        return semester.ToDto();
    }
}
