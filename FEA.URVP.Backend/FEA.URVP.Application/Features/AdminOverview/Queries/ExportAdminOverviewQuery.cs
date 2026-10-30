using FEA.URVP.Application.Features.Users.DTOs;
using MediatR;

namespace FEA.URVP.Application.Features.AdminOverview.Queries;

public sealed record ExportAdminOverviewQuery : IRequest<UserExportFileDto>;
