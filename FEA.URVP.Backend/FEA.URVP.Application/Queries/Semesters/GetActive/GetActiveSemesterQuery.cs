using FEA.URVP.Application.DTOs.Semesters;
using MediatR;

namespace FEA.URVP.Application.Queries.Semesters.GetActive;

public sealed record GetActiveSemesterQuery : IRequest<SemesterDto?>;
