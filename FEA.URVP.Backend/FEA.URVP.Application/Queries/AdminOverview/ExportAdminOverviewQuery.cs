using FEA.URVP.Application.DTOs.Users;
using MediatR;

namespace FEA.URVP.Application.Queries.AdminOverview;

public sealed record ExportAdminOverviewQuery : IRequest<UserExportFileDto>;
