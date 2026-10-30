using FEA.URVP.Application.Features.ProjectRankings.DTOs;
using MediatR;

namespace FEA.URVP.Application.Features.ProjectRankings.Commands.Upsert;

public sealed class UpsertProjectRankingCommand : IRequest<ProjectRankingDto>
{
    public Guid ProjectId { get; set; }
    public byte Rank { get; set; }

    /// <summary>Set by the API from the authenticated principal.</summary>
    public Guid CurrentUserId { get; set; }
}
