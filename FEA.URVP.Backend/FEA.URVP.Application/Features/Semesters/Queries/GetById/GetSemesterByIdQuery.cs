using FEA.URVP.Application.Features.Semesters.DTOs;
using MediatR;

namespace FEA.URVP.Application.Features.Semesters.Queries.GetById;

public sealed record GetSemesterByIdQuery(Guid Id) : IRequest<SemesterDto>;
