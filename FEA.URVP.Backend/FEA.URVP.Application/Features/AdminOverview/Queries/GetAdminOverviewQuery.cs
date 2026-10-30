using FEA.URVP.Application.Features.AdminOverview.DTOs;
using MediatR;

namespace FEA.URVP.Application.Features.AdminOverview.Queries;

public sealed record GetAdminOverviewQuery : IRequest<AdminOverviewDto>;
