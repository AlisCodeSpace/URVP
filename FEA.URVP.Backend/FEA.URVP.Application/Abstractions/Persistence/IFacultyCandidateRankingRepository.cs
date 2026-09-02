using FEA.URVP.Domain.Entities.FacultyCandidateRankings;

namespace FEA.URVP.Application.Abstractions.Persistence;

public interface IFacultyCandidateRankingRepository
{
    Task<IReadOnlyList<FacultyCandidateRanking>> ListByProjectAsync(
        Guid projectId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FacultyCandidateRanking>> ListByProjectIdsAsync(
        IReadOnlyCollection<Guid> projectIds,
        CancellationToken cancellationToken = default);

    Task<FacultyCandidateRanking?> FindByProjectAndStudentAsync(
        Guid projectId,
        Guid studentUserId,
        CancellationToken cancellationToken = default);

    void Add(FacultyCandidateRanking ranking);

    void Remove(FacultyCandidateRanking ranking);
}
