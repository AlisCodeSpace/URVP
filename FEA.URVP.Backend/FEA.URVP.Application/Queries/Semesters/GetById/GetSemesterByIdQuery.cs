using FEA.URVP.Application.DTOs.Semesters;
using MediatR;

namespace FEA.URVP.Application.Queries.Semesters.GetById;

public sealed record GetSemesterByIdQuery(Guid Id) : IRequest<SemesterDto>;
