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
    private readonly IProjectResearchAssistantRepository _assistants;

    public GetStudentProfileByUserIdQueryHandler(
        IStudentProfileRepository profiles,
        IUserRepository users,
        IFileStorageRepository files,
        IProjectRankingRepository rankings,
        IMatchingRunRepository placements,
        IProjectAlumniRepository alumni,
        IProjectResearchAssistantRepository assistants)
    {
        _profiles = profiles;
        _users = users;
        _files = files;
        _rankings = rankings;
        _placements = placements;
        _alumni = alumni;
        _assistants = assistants;
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

        if (viewer.Role is not UserRole.Admin)
        {
            var allowed = viewer.Role is UserRole.Faculty
                && await StudentProfileAccess.FacultyMayViewStudentAsync(
                    viewer.Id,
                    request.StudentUserId,
                    _rankings,
                    _placements,
                    _alumni,
                    cancellationToken);

            if (!allowed)
            {
                allowed = await _assistants.CanViewStudentAsync(
                    viewer.Email,
                    request.StudentUserId,
                    cancellationToken);
            }

            if (!allowed)
            {
                throw new UnauthorizedAccessException(
                    "You can only view profiles of students who ranked or were assigned to a project you posted or assist.");
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

        string? cvName = null;
        if (profile.CvFileId is Guid cvId)
        {
            cvName = (await _files.FindByIdAsync(cvId, cancellationToken))?.FileName;
        }

        return profile.ToDto(student, transcriptName, cvName);
    }
}
