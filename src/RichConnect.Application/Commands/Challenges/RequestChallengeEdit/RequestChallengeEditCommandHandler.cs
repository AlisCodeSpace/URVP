using Microsoft.EntityFrameworkCore;
using RICHConnect.Backend.Application.Common;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Challenges.Interfaces;
using RICHConnect.Backend.Domain.Events;
using RICHConnect.Backend.Application.DTOs.Challenge;
using RICHConnect.Backend.Infrastructure.Events;
using RICHConnect.Backend.Domain.Enums;
using RICHConnect.Backend.Domain.Entities.Challenges;

namespace RICHConnect.Backend.Application.Commands.RequestChallengeEdit
{
    /// <summary>
    /// Handler for RequestChallengeEditCommand
    /// </summary>
    public class RequestChallengeEditCommandHandler : BaseCommandHandler<RequestChallengeEditCommand, ChallengeEditRequestDto>
    {
        private readonly IChallengeRepository _challengeRepository;
        private readonly IChallengeEditRequestRepository _editRequestRepository;
        private readonly IEventBus _eventBus;

        public RequestChallengeEditCommandHandler(
            IChallengeRepository challengeRepository,
            IChallengeEditRequestRepository editRequestRepository,
            IEventBus eventBus,
            ILogger<RequestChallengeEditCommandHandler> logger,
            AppDbContext context) : base(logger, context)
        {
            _challengeRepository = challengeRepository;
            _editRequestRepository = editRequestRepository;
            _eventBus = eventBus;
        }

        // Enable transaction support for edit request creation
        protected override bool UseTransaction => true;

        protected override async Task<ChallengeEditRequestDto> HandleInternal(RequestChallengeEditCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling RequestChallengeEditCommand for ChallengeId: {ChallengeId}, RequestedBy: {RequestedBy}", 
                command.ChallengeId, command.RequestedBy);

            // 1. Validate that the challenge exists and belongs to the requesting user
            var challenge = await _challengeRepository.GetByIdAsync(command.ChallengeId);
            if (challenge == null)
            {
                throw new ArgumentException("Challenge not found");
            }

            if (challenge.SubmittedBy != command.RequestedBy)
            {
                throw new UnauthorizedAccessException("You can only request edits for your own challenges");
            }

            // 2. Validate that the challenge is in a state that allows edit requests
            if (challenge.Status == ChallengeStatus.Matched)
            {
                throw new InvalidOperationException("Cannot request edits for matched challenges");
            }

            // Check if there's already a pending edit request for this challenge
            if (await _editRequestRepository.HasPendingRequestsAsync(command.ChallengeId))
            {
                throw new InvalidOperationException("There is already a pending edit request for this challenge");
            }

            // Get user details for event and DTO (since GetByIdAsync doesn't include navigation properties)
            var requestingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == command.RequestedBy, cancellationToken);

            // 3. Create a new edit request record
            var editRequest = new ChallengeEditRequest
            {
                ChallengeId = command.ChallengeId,
                EditReason = command.EditReason,
                RequestedBy = command.RequestedBy,
                Status = EditRequestStatus.Pending
            };

            var createdRequest = await _editRequestRepository.CreateAsync(editRequest);

            // 4. Publish ChallengeEditRequestedEvent
            var domainEvent = new ChallengeEditRequestedEvent(
                command.ChallengeId,
                createdRequest.Id,
                challenge.Title,
                command.RequestedBy,
                requestingUser?.Name ?? "Unknown User",
                requestingUser?.Email ?? "unknown@example.com",
                command.EditReason,
                challenge.Status.ToString()
            );

            await _eventBus.PublishAsync(domainEvent);

            // 5. Return the created edit request DTO
            return new ChallengeEditRequestDto
            {
                Id = createdRequest.Id,
                ChallengeId = createdRequest.ChallengeId,
                EditReason = createdRequest.EditReason,
                RequestedBy = createdRequest.RequestedBy,
                RequestedByName = requestingUser?.Name ?? "Unknown User",
                RequestedAt = createdRequest.RequestedAt,
                Status = (int)createdRequest.Status,
                AdminResponse = createdRequest.AdminResponse,
                RespondedAt = createdRequest.RespondedAt,
                RespondedBy = createdRequest.RespondedBy
            };
        }
    }
}
