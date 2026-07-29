using MediatR;
using RICHConnect.Backend.Application.Common;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Themes.Interfaces;
using RICHConnect.Backend.Application.Commands.Notifications.CreateNotification;
using RICHConnect.Backend.Application.Services.Notifications;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyThemeRejected
{
    public class NotifyThemeRejectedCommandHandler : BaseCommandHandler<NotifyThemeRejectedCommand>
    {
        private readonly IThemeRepository _themeRepository;
        private readonly IMediator _mediator;

        public NotifyThemeRejectedCommandHandler(
            IThemeRepository themeRepository,
            IMediator mediator,
            ILogger<NotifyThemeRejectedCommandHandler> logger,
            AppDbContext context)
            : base(logger, context)
        {
            _themeRepository = themeRepository;
            _mediator = mediator;
        }

        protected override async Task HandleInternal(NotifyThemeRejectedCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling NotifyThemeRejectedCommand for theme {ThemeId}", request.ThemeId);

            var theme = await _themeRepository.GetThemeWithUserAsync(request.ThemeId);

            if (theme?.UserSubmitted == null)
            {
                _logger.LogWarning("Theme {ThemeId} not found for notification", request.ThemeId);
                return;
            }

            // Create notification for theme submitter using MediatR
            var command = new CreateNotificationCommand
            {
                UserId = theme.SubmittedBy,
                Title = NotificationMessages.Theme.RejectedTitle(),
                Message = NotificationMessages.Theme.RejectedMessage(theme.Title, request.RejectionReason),
                Type = NotificationType.ThemeRejected,
                Link = $"/themes/{theme.Id}",
                Priority = "high"
            };

            await _mediator.Send(command, cancellationToken);

            _logger.LogInformation("Successfully created notification for theme rejection {ThemeId}", request.ThemeId);
        }
    }
}
