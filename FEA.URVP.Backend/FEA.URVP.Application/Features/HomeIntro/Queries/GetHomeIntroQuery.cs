using FEA.URVP.Application.Features.HomeIntro.DTOs;
using MediatR;

namespace FEA.URVP.Application.Features.HomeIntro.Queries;

public sealed record GetHomeIntroQuery : IRequest<HomeIntroDto>;
