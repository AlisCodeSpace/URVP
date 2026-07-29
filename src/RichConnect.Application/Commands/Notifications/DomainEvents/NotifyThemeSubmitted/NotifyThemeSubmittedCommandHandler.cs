using MediatR;
using RICHConnect.Backend.Application.Common;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Themes.Interfaces;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Users.Interfaces;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Application.Commands.Notifications.CreateNotification;
using RICHConnect.Backend.Application.Services.Notifications;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyThemeSubmitted
{
    public class NotifyThemeSubmittedCommandHandler : BaseCommandHandler<NotifyThemeSubmittedCommand>
    {
        private readonly IThemeRepository _themeRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMediator _mediator;

        public NotifyThemeSubmittedCommandHandler(
            IThemeRepository themeRepository,
            IUserRepository userRepository,
            IMediator mediator,
            ILogger<NotifyThemeSubmittedCommandHandler> logger,
            AppDbContext context)
            : base(logger, context)
        {
            _themeRepository = themeRepository;
            _userRepository = userRepository;
            _mediator = mediator;
        }

        protected override async Task HandleInternal(NotifyThemeSubmittedCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling NotifyThemeSubmittedCommand for theme {ThemeId}", request.ThemeId);

            var theme = await _themeRepository.GetThemeWithUserAsync(request.ThemeId);

            if (theme?.UserSubmitted == null)
            {
                _logger.LogWarning("Theme {ThemeId} not found for notification", request.ThemeId);
                return;
            }

            // Get all admin users
            var adminUsers = await _userRepository.GetAdminUserIdsAsync();

            if (!adminUsers.Any())
            {
                _logger.LogWarning("No admin users found for theme submission notification");
                return;
            }

            // Create notifications for admins using MediatR
            foreach (var adminId in adminUsers)
            {
                var command = new CreateNotificationCommand
                {
                    UserId = adminId,
                    Title = NotificationMessages.Theme.SubmittedTitle(),
                    Message = NotificationMessages.Theme.SubmittedMessage(theme.Title),
                    Type = NotificationType.ThemeSubmitted,
                    Link = $"/themes/{theme.Id}",
                    Priority = "medium",
                    ReferenceId = theme.Id,
                    ReferenceType = "Theme"
                };
                
                await _mediator.Send(command, cancellationToken);
            }

            _logger.LogInformation("Successfully created {Count} notifications for theme submission {ThemeId}", 
                adminUsers.Count, request.ThemeId);
        }
    }
}
