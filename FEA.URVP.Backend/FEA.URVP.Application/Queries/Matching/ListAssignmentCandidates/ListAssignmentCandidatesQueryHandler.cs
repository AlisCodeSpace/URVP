using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.DTOs.Matching;
using FEA.URVP.Application.Matching;
using MediatR;

namespace FEA.URVP.Application.Queries.Matching.ListAssignmentCandidates;

public sealed class ListAssignmentCandidatesQueryHandler
    : IRequestHandler<ListAssignmentCandidatesQuery, (IReadOnlyList<AssignmentCandidateDto> Items, int TotalCount)>
{
    private readonly IProjectRepository _projects;
    private readonly IAssignmentCandidateReadRepository _candidates;

    public ListAssignmentCandidatesQueryHandler(
        IProjectRepository projects,
        IAssignmentCandidateReadRepository candidates)
    {
        _projects = projects;
        _candidates = candidates;
    }

    public async Task<(IReadOnlyList<AssignmentCandidateDto> Items, int TotalCount)> Handle(
        ListAssignmentCandidatesQuery request,
        CancellationToken cancellationToken)
    {
        var project = await _projects.FindByIdAsync(request.ProjectId, cancellationToken)
            ?? throw new KeyNotFoundException($"Project {request.ProjectId} was not found.");

        if (!request.RecommendedOnly)
        {
            var (items, totalCount) = await _candidates.PageEligibleAsync(
                project.Id,
                request.Search,
                request.PageNumber,
                request.PageSize,
                cancellationToken);
            var labels = await _candidates.LabelsForStudentsAsync(
                items.Select(item => item.UserId).ToArray(),
                cancellationToken);

            return (items.Select(item => ToDto(item, project.ResearchAreas, project.MinQualifications, labels)).ToList(), totalCount);
        }

        var eligible = await _candidates.ListEligibleAsync(project.Id, request.Search, cancellationToken);
        var ranked = eligible
            .Select(student => (
                student,
                fit: AssignmentCandidateMatching.Evaluate(
                    project.ResearchAreas,
                    project.MinQualifications,
                    student.ResearchTopics,
                    student.Major,
                    student.Faculty)))
            .Where(candidate => candidate.fit.IsRecommended)
            .OrderByDescending(candidate => candidate.fit.Score)
            .ThenBy(candidate => candidate.student.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var page = ranked
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();
        var pageLabels = await _candidates.LabelsForStudentsAsync(
            page.Select(candidate => candidate.student.UserId).ToArray(),
            cancellationToken);

        return (
            page.Select(candidate => ToDto(
                candidate.student,
                project.ResearchAreas,
                project.MinQualifications,
                pageLabels,
                candidate.fit)).ToList(),
            ranked.Count);
    }

    private static AssignmentCandidateDto ToDto(
        AssignmentStudentSource student,
        IReadOnlyList<string> researchAreas,
        string? minQualifications,
        IReadOnlyDictionary<Guid, ConfirmedPlacementLabel> labels,
        AssignmentCandidateMatching.Fit? fit = null)
    {
        fit ??= AssignmentCandidateMatching.Evaluate(
            researchAreas,
            minQualifications,
            student.ResearchTopics,
            student.Major,
            student.Faculty);
        labels.TryGetValue(student.UserId, out var label);

        return new AssignmentCandidateDto
        {
            UserId = student.UserId,
            Name = student.Name,
            Email = student.Email,
            Faculty = student.Faculty,
            Major = student.Major,
            Degree = student.Degree,
            HasProfile = student.HasProfile,
            MatchedResearchTopics = fit.MatchedResearchTopics,
            QualificationsMentionMajor = fit.QualificationsMentionMajor,
            QualificationsMentionFaculty = fit.QualificationsMentionFaculty,
            AssignedProjectId = label?.ProjectId,
            AssignedProjectTitle = label?.Title,
        };
    }
}
