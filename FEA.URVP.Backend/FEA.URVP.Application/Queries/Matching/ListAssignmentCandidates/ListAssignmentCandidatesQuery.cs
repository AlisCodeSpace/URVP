using FEA.URVP.Application.DTOs.Matching;
using MediatR;

namespace FEA.URVP.Application.Queries.Matching.ListAssignmentCandidates;

public sealed record ListAssignmentCandidatesQuery(
    Guid ProjectId,
    string? Search,
    bool RecommendedOnly,
    int PageNumber,
    int PageSize) : IRequest<(IReadOnlyList<AssignmentCandidateDto> Items, int TotalCount)>;
