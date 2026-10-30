using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.DTOs.Users;
using FEA.URVP.Application.Mappings;
using MediatR;

namespace FEA.URVP.Application.Queries.Users.GetProjectActivity;

public sealed class GetStudentProjectActivityQueryHandler
    : IRequestHandler<GetStudentProjectActivityQuery, StudentProjectActivityDto>
{
    private readonly IUserRepository _users;
    private readonly IProjectRankingRepository _rankings;
    private readonly IMatchingRunRepository _runs;
    private readonly ISemesterRepository _semesters;

    public GetStudentProjectActivityQueryHandler(
        IUserRepository users,
        IProjectRankingRepository rankings,
        IMatchingRunRepository runs,
        ISemesterRepository semesters)
    {
        _users = users;
        _rankings = rankings;
        _runs = runs;
        _semesters = semesters;
    }

    public async Task<StudentProjectActivityDto> Handle(
        GetStudentProjectActivityQuery request,
        CancellationToken cancellationToken)
    {
        var student = await _users.FindByIdAsync(request.StudentUserId, cancellationToken)
            ?? throw new KeyNotFoundException($"User {request.StudentUserId} was not found.");

        var active = await _semesters.FindActiveAsync(cancellationToken);
        var rankings = (await _rankings.ListByStudentAsync(student.Id, cancellationToken))
            .Where(ranking => active is not null && ranking.Project.SemesterId == active.Id)
            .OrderBy(ranking => ranking.Rank)
            .ThenBy(ranking => ranking.CreatedAt)
            .ToList();
        var matchedProjectIds = active is null
            ? []
            : (await _runs.ListConfirmedProjectIdsByStudentAsync(student.Id, active.Id, cancellationToken))
                .ToHashSet();
        var assignments = await _runs.ListConfirmedByStudentIdsAsync([student.Id], cancellationToken);

        return new StudentProjectActivityDto
        {
            Rankings = rankings.Select(ranking => ranking.ToDto(matchedProjectIds.Contains(ranking.ProjectId))).ToList(),
            Assignments = assignments
                .OrderBy(placement => placement.Project.Title)
                .Select(placement => placement.ToDto())
                .ToList(),
        };
    }
}
