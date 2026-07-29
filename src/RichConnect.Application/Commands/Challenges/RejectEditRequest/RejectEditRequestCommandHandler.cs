using RICHConnect.Backend.Application.Common;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Challenges.Interfaces;
using RICHConnect.Backend.Domain.Events;
using RICHConnect.Backend.Application.DTOs.Challenge;
using RICHConnect.Backend.Infrastructure.Events;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Commands.RejectEditRequest
{
    /// <summary>
    /// Handler for RejectEditRequestCommand
    /// </summary>
    public class RejectEditRequestCommandHandler : BaseCommandHandler<RejectEditRequestCommand, ChallengeEditRequestDto>
    {
        private readonly IChallengeEditRequestRepository _editRequestRepository;
        private readonly IEventBus _eventBus;

        public RejectEditRequestCommandHandler(
            IChallengeEditRequestRepository editRequestRepository,
            IEventBus eventBus,
            ILogger<RejectEditRequestCommandHandler> logger,
            AppDbContext context) : base(logger, context)
        {
            _editRequestRepository = editRequestRepository;
            _eventBus = eventBus;
        }

        // Enable transaction support for edit request rejection
        protected override bool UseTransaction => true;

        protected override async Task<ChallengeEditRequestDto> HandleInternal(RejectEditRequestCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling RejectEditRequestCommand for EditRequestId: {EditRequestId}, AdminId: {AdminId}", 
                command.EditRequestId, command.AdminId);

            // 1. Get the edit request
            var editRequest = await _editRequestRepository.GetByIdAsync(command.EditRequestId);
            if (editRequest == null)
            {
                throw new ArgumentException("Edit request not found");
            }

            // 2. Validate that the edit request is in pending status
            if (editRequest.Status != EditRequestStatus.Pending)
            {
                throw new InvalidOperationException("Only pending edit requests can be rejected");
            }

            // 3. Update the edit request status
            editRequest.Status = EditRequestStatus.Rejected;
            editRequest.AdminResponse = command.AdminResponse;
            editRequest.RespondedBy = command.AdminId;
            editRequest.RespondedAt = DateTime.UtcNow;
            editRequest.UpdatedAt = DateTime.UtcNow;

            var updatedRequest = await _editRequestRepository.UpdateAsync(editRequest);

            // 4. Publish ChallengeEditRequestRejectedEvent
            var rejectedEvent = new ChallengeEditRequestRejectedEvent(
                editRequest.Id,
                editRequest.ChallengeId,
                editRequest.RequestedBy,
                command.AdminId,
                editRequest.RespondedAt.Value,
                command.AdminResponse);

            await _eventBus.PublishAsync(rejectedEvent);

            _logger.LogInformation("Successfully rejected edit request {EditRequestId} for challenge {ChallengeId}", 
                editRequest.Id, editRequest.ChallengeId);

            // 5. Map to DTO and return
            return new ChallengeEditRequestDto
            {
                Id = updatedRequest.Id,
                ChallengeId = updatedRequest.ChallengeId,
                EditReason = updatedRequest.EditReason,
                RequestedBy = updatedRequest.RequestedBy,
                RequestedByName = updatedRequest.RequestedByUser?.Name,
                RequestedAt = updatedRequest.RequestedAt,
                Status = (int)updatedRequest.Status,
                AdminResponse = updatedRequest.AdminResponse,
                RespondedAt = updatedRequest.RespondedAt,
                RespondedBy = updatedRequest.RespondedBy
            };
        }
    }
}
