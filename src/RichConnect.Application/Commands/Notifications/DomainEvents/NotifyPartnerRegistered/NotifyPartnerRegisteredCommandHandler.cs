using MediatR;
using RICHConnect.Backend.Application.Common;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Partners.Interfaces;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Users.Interfaces;
using RICHConnect.Backend.Application.Commands.Notifications.CreateNotification;
using RICHConnect.Backend.Application.Services.Notifications;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyPartnerRegistered
{
    public class NotifyPartnerRegisteredCommandHandler : BaseCommandHandler<NotifyPartnerRegisteredCommand>
    {
        private readonly IPartnerRepository _partnerRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMediator _mediator;

        public NotifyPartnerRegisteredCommandHandler(
            IPartnerRepository partnerRepository,
            IUserRepository userRepository,
            IMediator mediator,
            ILogger<NotifyPartnerRegisteredCommandHandler> logger,
            AppDbContext context)
            : base(logger, context)
        {
            _partnerRepository = partnerRepository;
            _userRepository = userRepository;
            _mediator = mediator;
        }

        protected override async Task HandleInternal(NotifyPartnerRegisteredCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling NotifyPartnerRegisteredCommand for partner {PartnerId}", request.PartnerId);

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
                _logger.LogWarning("No admin users found for partner registration notification");
                return;
            }

            // Create notifications for admins using MediatR
            foreach (var adminId in adminUsers)
            {
                var command = new CreateNotificationCommand
                {
                    UserId = adminId,
                    Title = NotificationMessages.Partner.RegisteredTitle(),
                    Message = NotificationMessages.Partner.RegisteredMessage(partner.InstitutionName),
                    Type = NotificationType.PartnerRegistered,
                    Link = $"/partners/{partner.Id}",
                    Priority = "medium",
                    ReferenceId = partner.Id,
                    ReferenceType = "Partner"
                };
                
                await _mediator.Send(command, cancellationToken);
            }

            _logger.LogInformation("Successfully created {Count} notifications for partner registration {PartnerId}", 
                adminUsers.Count, request.PartnerId);
        }
    }
}
