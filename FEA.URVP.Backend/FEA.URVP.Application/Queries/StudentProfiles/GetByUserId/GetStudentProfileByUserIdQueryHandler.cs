using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.DTOs.StudentProfiles;
using FEA.URVP.Application.Mappings;
using FEA.URVP.Application.StudentProfiles;
using FEA.URVP.Domain.Enums;
using MediatR;

namespace FEA.URVP.Application.Queries.StudentProfiles.GetByUserId;

public sealed class GetStudentProfileByUserIdQueryHandler
    : IRequestHandler<GetStudentProfileByUserIdQuery, StudentProfileDto>
{
    private readonly IStudentProfileRepository _profiles;
    private readonly IUserRepository _users;
    private readonly IFileStorageRepository _files;
    private readonly IProjectRankingRepository _rankings;
    private readonly IMatchingRunRepository _placements;
    private readonly IProjectAlumniRepository _alumni;

    public GetStudentProfileByUserIdQueryHandler(
        IStudentProfileRepository profiles,
        IUserRepository users,
        IFileStorageRepository files,
        IProjectRankingRepository rankings,
        IMatchingRunRepository placements,
        IProjectAlumniRepository alumni)
    {
        _profiles = profiles;
        _users = users;
        _files = files;
        _rankings = rankings;
        _placements = placements;
        _alumni = alumni;
    }

    public async Task<StudentProfileDto> Handle(
        GetStudentProfileByUserIdQuery request,
        CancellationToken cancellationToken)
    {
        if (request.CurrentUserId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Authenticated user is required.");
        }

        var viewer = await _users.FindByIdAsync(request.CurrentUserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("User not found.");

        StudentProfileAccess.EnsureCanViewRankedStudent(viewer.Role);

        if (viewer.Role is not UserRole.Admin)
        {
            await StudentProfileAccess.EnsureFacultyMayViewStudentAsync(
                viewer.Id,
                request.StudentUserId,
                _rankings,
                _placements,
                _alumni,
                cancellationToken);
        }

        var student = await _users.FindByIdAsync(request.StudentUserId, cancellationToken)
            ?? throw new KeyNotFoundException($"Student {request.StudentUserId} was not found.");

        var profile = await _profiles.FindByUserIdAsync(student.Id, cancellationToken);
        if (profile is null)
        {
            return StudentProfileMappings.EmptyFromUser(student);
        }

        string? transcriptName = null;
        if (profile.TranscriptFileId is Guid transcriptId)
        {
            transcriptName = (await _files.FindByIdAsync(transcriptId, cancellationToken))?.FileName;
        }

        string? cvName = null;
        if (profile.CvFileId is Guid cvId)
        {
            cvName = (await _files.FindByIdAsync(cvId, cancellationToken))?.FileName;
        }

        return profile.ToDto(student, transcriptName, cvName);
    }
}
