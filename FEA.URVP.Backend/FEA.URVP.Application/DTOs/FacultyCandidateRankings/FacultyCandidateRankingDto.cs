namespace FEA.URVP.Application.DTOs.FacultyCandidateRankings;

public sealed class FacultyCandidateRankingDto
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public Guid StudentUserId { get; init; }
    public byte Rank { get; init; }
    public DateTime RankedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
