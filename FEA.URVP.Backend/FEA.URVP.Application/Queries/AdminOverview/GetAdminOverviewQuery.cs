using FEA.URVP.Application.DTOs.AdminOverview;
using MediatR;

namespace FEA.URVP.Application.Queries.AdminOverview;

public sealed record GetAdminOverviewQuery : IRequest<AdminOverviewDto>;
