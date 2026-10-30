using FEA.URVP.Application.Commands.Notifications.Create;
using FEA.URVP.Application.Commands.Notifications.Delete;
using FEA.URVP.Application.Commands.Notifications.DeleteAll;
using FEA.URVP.Application.Commands.Notifications.MarkAllAsRead;
using FEA.URVP.Application.Commands.Notifications.MarkAsRead;
using FEA.URVP.Application.Commands.Notifications.NotifyApplicationWindowClosed;
using FEA.URVP.Application.Commands.Notifications.NotifyApplicationWindowOpened;
using FEA.URVP.Application.Commands.Notifications.NotifyNewsPublished;
using FEA.URVP.Application.Commands.Notifications.NotifyPlacementCancelled;
using FEA.URVP.Application.Commands.Notifications.NotifyPlacementDeclined;
using FEA.URVP.Application.Commands.Notifications.NotifyProjectClosed;
using FEA.URVP.Application.Commands.Notifications.NotifyProjectDeleted;
using FEA.URVP.Application.Commands.Notifications.NotifyProjectOpen;
using FEA.URVP.Application.Commands.Notifications.NotifyRankingRemoved;
using FEA.URVP.Application.Commands.Notifications.NotifyRankingSubmitted;
using FEA.URVP.Application.Commands.Notifications.NotifyRoleAssigned;
using FEA.URVP.Application.Commands.Notifications.NotifySemesterCycleStarted;
using FEA.URVP.Application.Commands.Notifications.NotifyStudentProfileSubmitted;
using FEA.URVP.Application.Commands.Notifications.NotifyWorkshopAnnounced;
using FEA.URVP.Application.Commands.Notifications.UpdateSettings;
using FEA.URVP.Domain.Entities.Notifications;
using FluentValidation;

namespace FEA.URVP.Application.Validators.Notifications;

public sealed class CreateNotificationCommandValidator : AbstractValidator<CreateNotificationCommand>
{
    public CreateNotificationCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(Notification.TitleMaxLength);

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required.")
            .MaximumLength(Notification.MessageMaxLength);

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Type is not a valid notification type.");

        RuleFor(x => x.Priority)
            .IsInEnum()
            .When(x => x.Priority.HasValue);

        RuleFor(x => x.Link)
            .MaximumLength(Notification.DataMaxLength)
            .When(x => x.Link is not null);

        RuleFor(x => x.ReferenceType)
            .MaximumLength(Notification.ReferenceTypeMaxLength)
            .When(x => x.ReferenceType is not null);
    }
}

public sealed class MarkAsReadCommandValidator : AbstractValidator<MarkAsReadCommand>
{
    public MarkAsReadCommandValidator()
    {
        RuleFor(x => x.NotificationId)
            .NotEmpty().WithMessage("NotificationId is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");
    }
}

public sealed class MarkAllAsReadCommandValidator : AbstractValidator<MarkAllAsReadCommand>
{
    public MarkAllAsReadCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");
    }
}

public sealed class DeleteNotificationCommandValidator : AbstractValidator<DeleteNotificationCommand>
{
    public DeleteNotificationCommandValidator()
    {
        RuleFor(x => x.NotificationId)
            .NotEmpty().WithMessage("NotificationId is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");
    }
}

public sealed class UpdateNotificationSettingsCommandValidator
    : AbstractValidator<UpdateNotificationSettingsCommand>
{
    public UpdateNotificationSettingsCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");
    }
}

public sealed class DeleteAllNotificationsCommandValidator
    : AbstractValidator<DeleteAllNotificationsCommand>
{
    public DeleteAllNotificationsCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");
    }
}

public sealed class NotifyProjectOpenCommandValidator : AbstractValidator<NotifyProjectOpenCommand>
{
    public NotifyProjectOpenCommandValidator() =>
        RuleFor(x => x.ProjectId).NotEmpty();
}

public sealed class NotifyProjectClosedCommandValidator : AbstractValidator<NotifyProjectClosedCommand>
{
    public NotifyProjectClosedCommandValidator() =>
        RuleFor(x => x.ProjectId).NotEmpty();
}

public sealed class NotifyProjectDeletedCommandValidator : AbstractValidator<NotifyProjectDeletedCommand>
{
    public NotifyProjectDeletedCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.OwnerUserId).NotEmpty();
        RuleFor(x => x.ProjectTitle).NotEmpty();
    }
}

public sealed class NotifyPlacementDeclinedCommandValidator : AbstractValidator<NotifyPlacementDeclinedCommand>
{
    public NotifyPlacementDeclinedCommandValidator() =>
        RuleFor(x => x.PlacementId).NotEmpty();
}

public sealed class NotifyPlacementCancelledCommandValidator : AbstractValidator<NotifyPlacementCancelledCommand>
{
    public NotifyPlacementCancelledCommandValidator() =>
        RuleFor(x => x.PlacementId).NotEmpty();
}

public sealed class NotifyRankingSubmittedCommandValidator : AbstractValidator<NotifyRankingSubmittedCommand>
{
    public NotifyRankingSubmittedCommandValidator()
    {
        RuleFor(x => x.RankingId).NotEmpty();
        RuleFor(x => x.OwnerUserId).NotEmpty();
    }
}

public sealed class NotifyRankingRemovedCommandValidator : AbstractValidator<NotifyRankingRemovedCommand>
{
    public NotifyRankingRemovedCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.OwnerUserId).NotEmpty();
    }
}

public sealed class NotifyApplicationWindowOpenedCommandValidator
    : AbstractValidator<NotifyApplicationWindowOpenedCommand>
{
    public NotifyApplicationWindowOpenedCommandValidator() =>
        RuleFor(x => x.SemesterId).NotEmpty();
}

public sealed class NotifyApplicationWindowClosedCommandValidator
    : AbstractValidator<NotifyApplicationWindowClosedCommand>
{
    public NotifyApplicationWindowClosedCommandValidator() =>
        RuleFor(x => x.SemesterId).NotEmpty();
}

public sealed class NotifySemesterCycleStartedCommandValidator
    : AbstractValidator<NotifySemesterCycleStartedCommand>
{
    public NotifySemesterCycleStartedCommandValidator() =>
        RuleFor(x => x.SemesterId).NotEmpty();
}

public sealed class NotifyNewsPublishedCommandValidator : AbstractValidator<NotifyNewsPublishedCommand>
{
    public NotifyNewsPublishedCommandValidator() =>
        RuleFor(x => x.ArticleId).NotEmpty();
}

public sealed class NotifyWorkshopAnnouncedCommandValidator : AbstractValidator<NotifyWorkshopAnnouncedCommand>
{
    public NotifyWorkshopAnnouncedCommandValidator() =>
        RuleFor(x => x.WorkshopId).NotEmpty();
}

public sealed class NotifyRoleAssignedCommandValidator : AbstractValidator<NotifyRoleAssignedCommand>
{
    public NotifyRoleAssignedCommandValidator() =>
        RuleFor(x => x.UserId).NotEmpty();
}

public sealed class NotifyStudentProfileSubmittedCommandValidator
    : AbstractValidator<NotifyStudentProfileSubmittedCommand>
{
    public NotifyStudentProfileSubmittedCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.StudentName).NotEmpty();
    }
}
