using FEA.URVP.Application.Features.Semesters.DTOs;
using MediatR;

namespace FEA.URVP.Application.Features.Semesters.Queries.GetActive;

public sealed record GetActiveSemesterQuery : IRequest<SemesterDto?>;
