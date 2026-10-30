using FEA.URVP.Application.Features.Matching.DTOs;
using MediatR;

namespace FEA.URVP.Application.Features.Matching.Queries.ListAssignmentCandidates;

public sealed record ListAssignmentCandidatesQuery(
    Guid ProjectId,
    string? Search,
    bool RecommendedOnly,
    bool RankedProjectsOnly,
    int PageNumber,
    int PageSize) : IRequest<(IReadOnlyList<AssignmentCandidateDto> Items, int TotalCount)>;
