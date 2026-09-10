using FEA.URVP.Application.DTOs.HomeIntro;
using MediatR;

namespace FEA.URVP.Application.Queries.HomeIntro;

public sealed record GetHomeIntroQuery : IRequest<HomeIntroDto>;
