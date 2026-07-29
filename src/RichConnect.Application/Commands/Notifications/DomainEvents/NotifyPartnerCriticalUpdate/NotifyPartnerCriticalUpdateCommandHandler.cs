using MediatR;
using RICHConnect.Backend.Application.Common;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Partners.Interfaces;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Users.Interfaces;
using RICHConnect.Backend.Application.Commands.Notifications.CreateNotification;
using RICHConnect.Backend.Application.Services.Notifications;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyPartnerCriticalUpdate
{
    public class NotifyPartnerCriticalUpdateCommandHandler : BaseCommandHandler<NotifyPartnerCriticalUpdateCommand>
    {
        private readonly IPartnerRepository _partnerRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMediator _mediator;

        public NotifyPartnerCriticalUpdateCommandHandler(
            IPartnerRepository partnerRepository,
            IUserRepository userRepository,
            IMediator mediator,
            ILogger<NotifyPartnerCriticalUpdateCommandHandler> logger,
            AppDbContext context)
            : base(logger, context)
        {
            _partnerRepository = partnerRepository;
            _userRepository = userRepository;
            _mediator = mediator;
        }

        protected override async Task HandleInternal(NotifyPartnerCriticalUpdateCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling NotifyPartnerCriticalUpdateCommand for partner {PartnerId}", request.PartnerId);

            var partner = await _partnerRepository.GetPartnerWithUserAsync(request.PartnerId);

            if (partner?.User == null)
            {
                _logger.LogWarning("Partner {PartnerId} not found for notification", request.PartnerId);
                return;
            }

            // Get all admin users
            var adminUsers = await _userRepository.GetAdminUserIdsAsync();

            if (!adminUsers.Any())
            {
                _logger.LogWarning("No admin users found for partner critical update notification");
                return;
            }

            // Create notifications for admins using MediatR
            foreach (var adminId in adminUsers)
            {
                var command = new CreateNotificationCommand
                {
                    UserId = adminId,
                    Title = NotificationMessages.Partner.CriticalUpdateTitle(),
                    Message = NotificationMessages.Partner.CriticalUpdateMessage(partner.InstitutionName, string.Join(", ", request.CriticalFieldsChanged)),
                    Type = NotificationType.PartnerCriticalUpdate,
                    Link = $"/partners/{partner.Id}",
                    Priority = "high"
                };

                await _mediator.Send(command, cancellationToken);
            }

            _logger.LogInformation("Successfully created {Count} notifications for partner critical update {PartnerId}", 
                adminUsers.Count, request.PartnerId);
        }
    }
}
