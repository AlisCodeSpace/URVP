using FEA.URVP.Application.Abstractions.Events;
using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using FEA.URVP.Application.DTOs.StudentProfiles;
using FEA.URVP.Application.Mappings;
using FEA.URVP.Application.Notifications;
using FEA.URVP.Application.StudentProfiles;
using FEA.URVP.Domain.Catalog;
using FEA.URVP.Domain.Entities.StudentProfiles;
using FEA.URVP.Domain.Events.StudentProfiles;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.StudentProfiles.Upsert;

public sealed class UpsertStudentProfileCommandHandler
    : BaseCommandHandler<UpsertStudentProfileCommand, StudentProfileDto>
{
    private readonly IStudentProfileRepository _profiles;
    private readonly IUserRepository _users;
    private readonly IFileStorageRepository _files;
    private readonly IEventBus _eventBus;

    public UpsertStudentProfileCommandHandler(
        ILogger<UpsertStudentProfileCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IStudentProfileRepository profiles,
        IUserRepository users,
        IFileStorageRepository files,
        IEventBus eventBus)
        : base(logger, unitOfWork)
    {
        _profiles = profiles;
        _users = users;
        _files = files;
        _eventBus = eventBus;
    }

    protected override async Task<StudentProfileDto> HandleInternal(
        UpsertStudentProfileCommand request,
        CancellationToken cancellationToken)
    {
        if (request.CurrentUserId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Authenticated user is required.");
        }

        var user = await _users.FindByIdAsync(request.CurrentUserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("User not found.");

        StudentProfileAccess.EnsureCanManage(user.Role, user.Email);

        var transcript = await RequireOwnedDocumentAsync(
            request.TranscriptFileId,
            user.Id,
            FileStorageCatalog.CategoryTranscript,
            cancellationToken);

        string? citiFileName = null;
        if (request.CitiFileId is Guid citiId)
        {
            var citi = await RequireOwnedDocumentAsync(
                citiId,
                user.Id,
                FileStorageCatalog.CategoryCitiCertification,
                cancellationToken);
            citiFileName = citi.FileName;
        }

        var now = DateTime.UtcNow;
        var availability = request.Availability
            .Select(a => new DayAvailability
            {
                Day = a.Day.Trim(),
                Slots = a.Slots.Select(s => s.Trim()).Where(s => s.Length > 0).Distinct().ToList(),
            })
            .Where(a => a.Slots.Count > 0)
            .ToList();

        var profile = await _profiles.FindByUserIdAsync(user.Id, cancellationToken);
        var isNew = profile is null;
        if (profile is null)
        {
            profile = new StudentProfile
            {
                UserId = user.Id,
                CreatedAt = now,
            };
            _profiles.Add(profile);
        }

        profile.Gender = request.Gender.Trim();
        profile.MobileNumber = request.MobileNumber.Trim();
        profile.Degree = request.Degree.Trim();
        profile.ExpectedGraduationYear = request.ExpectedGraduationYear;
        profile.Languages = request.Languages.Select(l => l.Trim()).Where(l => l.Length > 0).Distinct().ToList();
        profile.OtherLanguages = NormalizeOptional(request.OtherLanguages);
        profile.CompletedCredits = request.CompletedCredits;
        profile.CumulativeAverage = request.CumulativeAverage;
        profile.ResearchTopics = request.ResearchTopics
            .Select(t => t.Trim())
            .Where(t => t.Length > 0)
            .Distinct()
            .ToList();
        profile.Publications = NormalizeOptional(request.Publications);
        profile.TranscriptFileId = transcript.Id;
        profile.CitiFileId = request.CitiFileId;
        profile.Availability = availability;
        profile.UpdatedAt = now;

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        Logger.LogInformation("Upserted student profile for user {UserId}", user.Id);

        if (isNew)
        {
            await NotificationEventPublish.TryPublishAsync(
                _eventBus,
                new StudentProfileSubmittedEvent(user.Id, user.Name),
                Logger,
                cancellationToken);
        }

        return profile.ToDto(user, transcript.FileName, citiFileName);
    }

    private async Task<Domain.Entities.Files.FileStorage> RequireOwnedDocumentAsync(
        Guid fileId,
        Guid userId,
        string expectedCategory,
        CancellationToken cancellationToken)
    {
        var file = await _files.FindByIdAsync(fileId, cancellationToken)
            ?? throw new ArgumentException($"File {fileId} was not found.");

        if (file.EntityType != FileStorageCatalog.EntityStudentProfile
            || file.EntityId != userId
            || file.FileCategory != expectedCategory)
        {
            throw new UnauthorizedAccessException("File does not belong to this student profile.");
        }

        return file;
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
