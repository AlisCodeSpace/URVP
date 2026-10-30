using FEA.URVP.Application.DTOs.Semesters;
using MediatR;

namespace FEA.URVP.Application.Queries.Semesters.List;

public sealed record ListSemestersQuery : IRequest<IReadOnlyList<SemesterDto>>;
