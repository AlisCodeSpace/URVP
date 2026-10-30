using FEA.URVP.Application.DTOs.FacultyCandidateRankings;
using MediatR;

namespace FEA.URVP.Application.Commands.FacultyCandidateRankings.Upsert;

public sealed class UpsertFacultyCandidateRankingCommand : IRequest<FacultyCandidateRankingDto>
{
    public Guid ProjectId { get; set; }
    public Guid StudentUserId { get; set; }
    public byte Rank { get; set; }

    /// <summary>Set by the API from the authenticated principal.</summary>
    public Guid CurrentUserId { get; set; }

    public bool IsAdmin { get; set; }
}
