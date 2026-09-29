using FEA.URVP.Application.DTOs.Users;
using FEA.URVP.Domain.Enums;
using MediatR;

namespace FEA.URVP.Application.Queries.Projects.Export;

public sealed class ExportAdminProjectsQuery : IRequest<UserExportFileDto>
{
    public string? Search { get; }
    public ProjectStatus? Status { get; }

    public ExportAdminProjectsQuery(string? search, ProjectStatus? status)
    {
        Search = search;
        Status = status;
    }
}
