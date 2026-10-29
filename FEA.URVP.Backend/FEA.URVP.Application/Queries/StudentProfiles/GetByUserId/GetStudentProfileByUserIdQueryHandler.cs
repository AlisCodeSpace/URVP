using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.DTOs.StudentProfiles;
using FEA.URVP.Application.Mappings;
using FEA.URVP.Application.StudentProfiles;
using MediatR;

namespace FEA.URVP.Application.Queries.StudentProfiles.GetByUserId;

public sealed class GetStudentProfileByUserIdQueryHandler
    : IRequestHandler<GetStudentProfileByUserIdQuery, StudentProfileDto>
{
    private readonly IStudentProfileRepository _profiles;
    private readonly IUserRepository _users;
    private readonly IFileStorageRepository _files;
    private readonly StudentProfileViewAccess _viewAccess;

    public GetStudentProfileByUserIdQueryHandler(
        IStudentProfileRepository profiles,
        IUserRepository users,
        IFileStorageRepository files,
        StudentProfileViewAccess viewAccess)
    {
        _profiles = profiles;
        _users = users;
        _files = files;
        _viewAccess = viewAccess;
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

        if (!await _viewAccess.CanViewAsync(viewer, request.StudentUserId, cancellationToken))
        {
            throw new UnauthorizedAccessException(
                "You can only view profiles of students who ranked or were assigned to a project you posted or assist.");
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
