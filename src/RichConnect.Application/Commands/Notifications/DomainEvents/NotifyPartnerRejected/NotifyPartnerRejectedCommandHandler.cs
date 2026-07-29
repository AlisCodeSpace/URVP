using MediatR;
using RICHConnect.Backend.Application.Common;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Partners.Interfaces;
using RICHConnect.Backend.Application.Commands.Notifications.CreateNotification;
using RICHConnect.Backend.Application.Services.Notifications;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyPartnerRejected
{
    public class NotifyPartnerRejectedCommandHandler : BaseCommandHandler<NotifyPartnerRejectedCommand>
    {
        private readonly IPartnerRepository _partnerRepository;
        private readonly IMediator _mediator;

        public NotifyPartnerRejectedCommandHandler(
            IPartnerRepository partnerRepository,
            IMediator mediator,
            ILogger<NotifyPartnerRejectedCommandHandler> logger,
            AppDbContext context)
            : base(logger, context)
        {
            _partnerRepository = partnerRepository;
            _mediator = mediator;
        }

        protected override async Task HandleInternal(NotifyPartnerRejectedCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling NotifyPartnerRejectedCommand for partner {PartnerId}", request.PartnerId);

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
                Title = NotificationMessages.Partner.RejectedTitle(),
                Message = NotificationMessages.Partner.RejectedMessage(partner.InstitutionName, request.RejectionReason),
                Type = NotificationType.PartnerRejected,
                Link = $"/partners/{partner.Id}",
                Priority = "high"
            };

            await _mediator.Send(command, cancellationToken);

            _logger.LogInformation("Successfully created notification for partner rejection {PartnerId}", request.PartnerId);
        }
    }
}
