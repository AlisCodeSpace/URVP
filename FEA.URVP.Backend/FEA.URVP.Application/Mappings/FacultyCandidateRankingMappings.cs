using FEA.URVP.Application.DTOs.FacultyCandidateRankings;
using FEA.URVP.Domain.Entities.FacultyCandidateRankings;

namespace FEA.URVP.Application.Mappings;

public static class FacultyCandidateRankingMappings
{
    public static FacultyCandidateRankingDto ToDto(this FacultyCandidateRanking ranking) =>
        new()
        {
            Id = ranking.Id,
            ProjectId = ranking.ProjectId,
            StudentUserId = ranking.StudentUserId,
            Rank = ranking.Rank,
            RankedAt = ranking.CreatedAt,
            UpdatedAt = ranking.UpdatedAt,
        };
}
