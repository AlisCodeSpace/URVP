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

    public GetStudentProfileByUserIdQueryHandler(
        IStudentProfileRepository profiles,
        IUserRepository users,
        IFileStorageRepository files,
        IProjectRankingRepository rankings)
    {
        _profiles = profiles;
        _users = users;
        _files = files;
        _rankings = rankings;
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
            var ranked = await _rankings.StudentHasRankedFacultyProjectAsync(
                request.StudentUserId,
                viewer.Id,
                cancellationToken);

            if (!ranked)
            {
                throw new UnauthorizedAccessException(
                    "You can only view profiles of students who ranked a project you posted.");
            }
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

        string? citiName = null;
        if (profile.CitiFileId is Guid citiId)
        {
            citiName = (await _files.FindByIdAsync(citiId, cancellationToken))?.FileName;
        }

        return profile.ToDto(student, transcriptName, citiName);
    }
}
