using MediatR;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Themes.Interfaces;
using RICHConnect.Backend.Application.Commands.Notifications.CreateNotification;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyThemeSubmissionConfirmation
{
    public class NotifyThemeSubmissionConfirmationCommandHandler : IRequestHandler<NotifyThemeSubmissionConfirmationCommand, Guid>
    {
        private readonly IMediator _mediator;
        private readonly IThemeRepository _themeRepository;
        private readonly ILogger<NotifyThemeSubmissionConfirmationCommandHandler> _logger;

        public NotifyThemeSubmissionConfirmationCommandHandler(
            IMediator mediator,
            IThemeRepository themeRepository,
            ILogger<NotifyThemeSubmissionConfirmationCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _themeRepository = themeRepository ?? throw new ArgumentNullException(nameof(themeRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Guid> Handle(NotifyThemeSubmissionConfirmationCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Creating theme submission confirmation notification for theme {ThemeId}", request.ThemeId);

                var theme = await _themeRepository.GetByIdAsync(request.ThemeId);
                if (theme == null)
                {
                    _logger.LogWarning("Theme {ThemeId} not found for submission confirmation notification", request.ThemeId);
                    throw new InvalidOperationException($"Theme {request.ThemeId} not found");
                }

                var createNotificationCommand = new CreateNotificationCommand
                {
                    UserId = request.SubmittedByUserId,
                    Title = "Theme Submitted Successfully",
                    Message = $"Your theme '{theme.Title}' has been submitted and is pending admin review.",
                    Type = NotificationType.ThemeSubmitted,
                    Link = $"/themes/{theme.Slug}",
                    Priority = "medium"
                };

                var notificationId = await _mediator.Send(createNotificationCommand, cancellationToken);

                _logger.LogInformation("Theme submission confirmation notification created: {NotificationId} for user {UserId}", 
                    notificationId, request.SubmittedByUserId);

                return notificationId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating theme submission confirmation notification for theme {ThemeId}", request.ThemeId);
                throw;
            }
        }
    }
}
