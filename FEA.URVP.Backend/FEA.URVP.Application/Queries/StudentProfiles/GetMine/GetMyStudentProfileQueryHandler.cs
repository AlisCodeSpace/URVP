using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.DTOs.StudentProfiles;
using FEA.URVP.Application.Mappings;
using FEA.URVP.Application.StudentProfiles;
using MediatR;

namespace FEA.URVP.Application.Queries.StudentProfiles.GetMine;

public sealed class GetMyStudentProfileQueryHandler
    : IRequestHandler<GetMyStudentProfileQuery, StudentProfileDto>
{
    private readonly IStudentProfileRepository _profiles;
    private readonly IUserRepository _users;
    private readonly IFileStorageRepository _files;

    public GetMyStudentProfileQueryHandler(
        IStudentProfileRepository profiles,
        IUserRepository users,
        IFileStorageRepository files)
    {
        _profiles = profiles;
        _users = users;
        _files = files;
    }

    public async Task<StudentProfileDto> Handle(
        GetMyStudentProfileQuery request,
        CancellationToken cancellationToken)
    {
        if (request.CurrentUserId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Authenticated user is required.");
        }

        var user = await _users.FindByIdAsync(request.CurrentUserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("User not found.");

        StudentProfileAccess.EnsureCanManage(user.Role, user.Email);

        var profile = await _profiles.FindByUserIdAsync(user.Id, cancellationToken);
        if (profile is null)
        {
            return StudentProfileMappings.EmptyFromUser(user);
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

        return profile.ToDto(user, transcriptName, citiName);
    }
}
