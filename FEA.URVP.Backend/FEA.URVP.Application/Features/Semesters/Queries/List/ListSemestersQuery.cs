using FEA.URVP.Application.Features.Semesters.DTOs;
using MediatR;

namespace FEA.URVP.Application.Features.Semesters.Queries.List;

public sealed record ListSemestersQuery : IRequest<IReadOnlyList<SemesterDto>>;
