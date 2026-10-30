using FEA.URVP.Application.Features.FacultyCandidateRankings.DTOs;
using FEA.URVP.Domain.Entities.FacultyCandidateRankings;

namespace FEA.URVP.Application.Features.FacultyCandidateRankings.Mappings;

public static class FacultyCandidateRankingMappings
{
    public static FacultyCandidateRankingDto ToDto(this FacultyCandidateRanking ranking) =>
        new()
        {
            Id = ranking.Id,
            ProjectId = ranking.ProjectId,
            StudentUserId = ranking.StudentUserId,
            Rank = ranking.Rank,
            RankedByUserId = ranking.RankedByUserId,
            RankedAt = ranking.CreatedAt,
            UpdatedAt = ranking.UpdatedAt,
        };
}
