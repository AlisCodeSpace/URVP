using FEA.URVP.Application.DTOs.ProjectRankings;
using MediatR;

namespace FEA.URVP.Application.Commands.ProjectRankings.Upsert;

public sealed class UpsertProjectRankingCommand : IRequest<ProjectRankingDto>
{
    public Guid ProjectId { get; set; }
    public byte Rank { get; set; }

    /// <summary>Set by the API from the authenticated principal.</summary>
    public Guid CurrentUserId { get; set; }
}
