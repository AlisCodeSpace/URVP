using MediatR;
using RICHConnect.Backend.Application.Common;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Partners.Interfaces;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Users.Interfaces;
using RICHConnect.Backend.Application.Commands.Notifications.CreateNotification;
using RICHConnect.Backend.Application.Services.Notifications;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyPartnerApproved
{
    public class NotifyPartnerApprovedCommandHandler : BaseCommandHandler<NotifyPartnerApprovedCommand>
    {
        private readonly IPartnerRepository _partnerRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMediator _mediator;

        public NotifyPartnerApprovedCommandHandler(
            IPartnerRepository partnerRepository,
            IUserRepository userRepository,
            IMediator mediator,
            ILogger<NotifyPartnerApprovedCommandHandler> logger,
            AppDbContext context)
            : base(logger, context)
        {
            _partnerRepository = partnerRepository;
            _userRepository = userRepository;
            _mediator = mediator;
        }

        protected override async Task HandleInternal(NotifyPartnerApprovedCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling NotifyPartnerApprovedCommand for partner {PartnerId}", request.PartnerId);

            var partner = await _partnerRepository.GetPartnerWithUserAsync(request.PartnerId);

            if (partner?.User == null)
            {
                _logger.LogWarning("Partner {PartnerId} not found for notification", request.PartnerId);
                return;
            }

            // Create notification for partner using MediatR
            var command = new CreateNotificationCommand
            {
                UserId = partner.UserId,
                Title = NotificationMessages.Partner.ApprovedTitle(),
                Message = NotificationMessages.Partner.ApprovedMessage(partner.InstitutionName),
                Type = NotificationType.PartnerApproved,
                Link = $"/partners/{partner.Id}",
                Priority = "medium"
            };

            await _mediator.Send(command, cancellationToken);

            _logger.LogInformation("Successfully created notification for partner approval {PartnerId}", request.PartnerId);
        }
    }
}
