using MediatR;
using RICHConnect.Backend.Application.Common;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Themes.Interfaces;
using RICHConnect.Backend.Application.Commands.Notifications.CreateNotification;
using RICHConnect.Backend.Application.Services.Notifications;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyThemeApproved
{
    public class NotifyThemeApprovedCommandHandler : BaseCommandHandler<NotifyThemeApprovedCommand>
    {
        private readonly IThemeRepository _themeRepository;
        private readonly IMediator _mediator;

        public NotifyThemeApprovedCommandHandler(
            IThemeRepository themeRepository,
            IMediator mediator,
            ILogger<NotifyThemeApprovedCommandHandler> logger,
            AppDbContext context)
            : base(logger, context)
        {
            _themeRepository = themeRepository;
            _mediator = mediator;
        }

        protected override async Task HandleInternal(NotifyThemeApprovedCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling NotifyThemeApprovedCommand for theme {ThemeId}", request.ThemeId);

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
                Title = NotificationMessages.Theme.ApprovedTitle(),
                Message = NotificationMessages.Theme.ApprovedMessage(theme.Title),
                Type = NotificationType.ThemeApproved,
                Link = $"/themes/{theme.Id}",
                Priority = "medium"
            };

            await _mediator.Send(command, cancellationToken);

            _logger.LogInformation("Successfully created notification for theme approval {ThemeId}", request.ThemeId);
        }
    }
}
